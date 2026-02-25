namespace RentalForge.Api.Models.Auth;

public record RegisterRequest
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string? Role { get; init; }
}
