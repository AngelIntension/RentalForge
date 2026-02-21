namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a rental store. Has a manager (Staff) and an address.
/// Circular reference: Store.ManagerStaffId -> Staff, Staff.StoreId -> Store.
/// </summary>
public class Store
{
    public int StoreId { get; set; }
    public int ManagerStaffId { get; set; }
    public int AddressId { get; set; }
    public DateTime LastUpdate { get; set; }

    public Staff ManagerStaff { get; set; } = null!;
    public Address Address { get; set; } = null!;
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Inventory> Inventories { get; set; } = [];
    public ICollection<Staff> Staff { get; set; } = [];
}
