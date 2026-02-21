namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a staff member. Belongs to a store and has an address.
/// Can manage a store (circular reference with Store.ManagerStaffId).
/// </summary>
public class Staff
{
    public int StaffId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int AddressId { get; set; }
    public string? Email { get; set; }
    public int StoreId { get; set; }
    public bool Active { get; set; }
    public string Username { get; set; } = null!;
    public string? Password { get; set; }
    public DateTime LastUpdate { get; set; }
    public byte[]? Picture { get; set; }

    public Address Address { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public ICollection<Store> ManagedStores { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Rental> Rentals { get; set; } = [];
}
