using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Models.Auth;

namespace RentalForge.Api.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    DvdrentalContext db,
    IConfiguration configuration,
    ILogger<AuthService> logger,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshRequest> refreshValidator) : IAuthService
{
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, ClaimsPrincipal? caller)
    {
        var validationResult = await registerValidator.ValidateAsync(request);
        var allErrors = validationResult.AsErrors();

        if (allErrors.Count > 0)
            return Result<AuthResponse>.Invalid(allErrors);

        // Check role elevation authorization
        var requestedRole = request.Role ?? "Customer";
        if (requestedRole is "Admin" or "Staff")
        {
            var isAdmin = caller?.IsInRole("Admin") == true;
            if (!isAdmin)
            {
                logger.LogWarning("Forbidden role elevation attempt to {Role}", requestedRole);
                return Result<AuthResponse>.Forbidden();
            }
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var identityErrors = createResult.Errors
                .Select(e => e.Code switch
                {
                    "DuplicateEmail" or "DuplicateUserName" => new ValidationError("Email", "Email is already registered."),
                    _ => new ValidationError("Password", e.Description)
                })
                .ToList();
            return Result<AuthResponse>.Invalid(identityErrors);
        }

        await userManager.AddToRoleAsync(user, requestedRole);

        logger.LogInformation("Registered user {Email} with role {Role}", user.Email, requestedRole);

        var token = GenerateJwtToken(user, requestedRole);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return Result<AuthResponse>.Created(new AuthResponse(
            token,
            refreshToken.Token,
            new UserDto(user.Id, user.Email!, requestedRole, user.CustomerId, user.CreatedAt)));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var validationResult = await loginValidator.ValidateAsync(request);
        var allErrors = validationResult.AsErrors();

        if (allErrors.Count > 0)
            return Result<AuthResponse>.Invalid(allErrors);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            logger.LogInformation("Failed login attempt for {Email}", request.Email);
            return Result<AuthResponse>.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Customer";

        logger.LogInformation("User {UserId} logged in", user.Id);

        var token = GenerateJwtToken(user, role);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return Result<AuthResponse>.Success(new AuthResponse(
            token,
            refreshToken.Token,
            new UserDto(user.Id, user.Email!, role, user.CustomerId, user.CreatedAt)));
    }

    public async Task<Result<RefreshResponse>> RefreshAsync(RefreshRequest request)
    {
        var validationResult = await refreshValidator.ValidateAsync(request);
        var allErrors = validationResult.AsErrors();

        if (allErrors.Count > 0)
            return Result<RefreshResponse>.Invalid(allErrors);

        var existingToken = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (existingToken is null)
            return Result<RefreshResponse>.Unauthorized();

        // Check if token was already consumed or revoked — potential reuse attack
        if (existingToken.IsUsed || existingToken.RevokedAt.HasValue)
        {
            logger.LogWarning("Refresh token reuse detected for user {UserId}, family {Family}. Invalidating family.",
                existingToken.UserId, existingToken.Family);
            await RevokeTokenFamilyAsync(existingToken.Family);
            return Result<RefreshResponse>.Unauthorized();
        }

        // Check expiry
        if (existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            logger.LogInformation("Expired refresh token rejected for user {UserId}", existingToken.UserId);
            return Result<RefreshResponse>.Unauthorized();
        }

        // Consume old token with optimistic concurrency
        existingToken.IsUsed = true;
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request already consumed this token — treat as reuse
            logger.LogWarning("Concurrent refresh token consumption detected for user {UserId}, family {Family}. Invalidating family.",
                existingToken.UserId, existingToken.Family);
            await RevokeTokenFamilyAsync(existingToken.Family);
            return Result<RefreshResponse>.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(existingToken.UserId);
        if (user is null)
            return Result<RefreshResponse>.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Customer";

        var newJwt = GenerateJwtToken(user, role);
        var newRefreshToken = await CreateRefreshTokenAsync(user.Id, existingToken.Family);

        logger.LogInformation("Refreshed tokens for user {UserId}, family {Family}", user.Id, existingToken.Family);

        return Result<RefreshResponse>.Success(new RefreshResponse(newJwt, newRefreshToken.Token));
    }

    public async Task<Result> LogoutAsync(LogoutRequest request, string userId)
    {
        var existingToken = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (existingToken is not null)
        {
            await RevokeTokenFamilyAsync(existingToken.Family);
            logger.LogInformation("User {UserId} logged out, revoked family {Family}", userId, existingToken.Family);
        }

        return Result.NoContent();
    }

    public async Task<Result<UserDto>> GetMeAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<UserDto>.NotFound();

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Customer";

        return Result<UserDto>.Success(
            new UserDto(user.Id, user.Email!, role, user.CustomerId, user.CreatedAt));
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationMinutes = int.TryParse(configuration["Jwt:AccessTokenExpirationMinutes"], out var m) ? m : 15;

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            ]),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(string userId, string? family = null)
    {
        var expirationDays = int.TryParse(configuration["Jwt:RefreshTokenExpirationDays"], out var d) ? d : 7;

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Family = family ?? Guid.NewGuid().ToString(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        return refreshToken;
    }

    private async Task RevokeTokenFamilyAsync(string family)
    {
        await db.RefreshTokens
            .Where(t => t.Family == family && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
    }
}
