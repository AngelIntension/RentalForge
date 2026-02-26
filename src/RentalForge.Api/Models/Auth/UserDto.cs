namespace RentalForge.Api.Models.Auth;

public record UserDto(
    string Id,
    string Email,
    string Role,
    int? CustomerId,
    int? StaffId,
    DateTime CreatedAt);
