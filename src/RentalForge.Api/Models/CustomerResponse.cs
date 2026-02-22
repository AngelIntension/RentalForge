namespace RentalForge.Api.Models;

/// <summary>
/// Response DTO for customer data.
/// </summary>
public record CustomerResponse(
    int Id,
    int StoreId,
    string FirstName,
    string LastName,
    string? Email,
    int AddressId,
    bool IsActive,
    DateOnly CreateDate,
    DateTime? LastUpdate);
