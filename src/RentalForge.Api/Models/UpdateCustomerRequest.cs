namespace RentalForge.Api.Models;

/// <summary>
/// Request DTO for updating an existing customer (full replacement).
/// </summary>
public record UpdateCustomerRequest
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? Email { get; init; }
    public int StoreId { get; init; }
    public int AddressId { get; init; }
}
