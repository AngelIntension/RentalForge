namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a rental customer. Belongs to a store and has an address.
/// </summary>
public class Customer
{
    public int CustomerId { get; set; }
    public int StoreId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }
    public int AddressId { get; set; }
    public bool Activebool { get; set; }
    public DateOnly CreateDate { get; set; }
    public DateTime? LastUpdate { get; set; }
    public int? Active { get; set; }

    public Store Store { get; set; } = null!;
    public Address Address { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Rental> Rentals { get; set; } = [];
    public ApplicationUser? AuthUser { get; set; }
}
