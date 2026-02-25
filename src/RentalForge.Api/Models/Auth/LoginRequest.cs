namespace RentalForge.Api.Models.Auth;

public record LoginRequest
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}
