using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Tests.Infrastructure;

public static class AuthTestHelper
{
    public const string DefaultPassword = "TestP@ss1";

    public static async Task<ApplicationUser> CreateTestUserAsync(
        IServiceProvider services,
        string email,
        string role,
        int? customerId = null)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Idempotent — return existing user if already created
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return existing;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Failed to create test user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(user, role);

        return user;
    }

    public static string GenerateTestJwtToken(
        string userId,
        string email,
        string role,
        int expirationMinutes = 15)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(TestWebAppFactory.TestJwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
            ]),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            SigningCredentials = credentials,
            Issuer = TestWebAppFactory.TestJwtIssuer,
            Audience = TestWebAppFactory.TestJwtAudience
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }

    public static HttpClient CreateAuthenticatedClient(
        TestWebAppFactory factory,
        string userId,
        string email,
        string role)
    {
        var client = factory.CreateClient();
        var token = GenerateTestJwtToken(userId, email, role);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static void SetAuthToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
