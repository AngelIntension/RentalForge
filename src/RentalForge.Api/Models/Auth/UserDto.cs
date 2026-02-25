namespace RentalForge.Api.Models.Auth;

public record UserDto(
    string Id,
    string Email,
    string Role,
    int? CustomerId,
    DateTime CreatedAt);
