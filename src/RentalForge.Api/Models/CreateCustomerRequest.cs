namespace RentalForge.Api.Models;

/// <summary>
/// Request DTO for creating a new customer.
/// </summary>
public record CreateCustomerRequest
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? Email { get; init; }
    public int StoreId { get; init; }
    public int AddressId { get; init; }
}
