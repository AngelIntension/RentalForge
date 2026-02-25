namespace RentalForge.Api.Models.Auth;

public record AuthResponse(
    string Token,
    string RefreshToken,
    UserDto User);
