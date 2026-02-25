namespace RentalForge.Api.Models.Auth;

public record RefreshRequest
{
    public string RefreshToken { get; init; } = null!;
}
