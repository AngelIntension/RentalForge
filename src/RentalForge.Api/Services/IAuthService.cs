using System.Security.Claims;
using Ardalis.Result;
using RentalForge.Api.Models.Auth;

namespace RentalForge.Api.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, ClaimsPrincipal? caller);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<RefreshResponse>> RefreshAsync(RefreshRequest request);
    Task<Result> LogoutAsync(LogoutRequest request, string userId);
    Task<Result<UserDto>> GetMeAsync(string userId);
}
