namespace RentalForge.Api.Models.Auth;

public record LogoutRequest
{
    public string RefreshToken { get; init; } = null!;
}
