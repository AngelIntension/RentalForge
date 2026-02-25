namespace RentalForge.Api.Models.Auth;

public record RefreshResponse(
    string Token,
    string RefreshToken);
