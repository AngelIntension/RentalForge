namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a physical address. Referenced by customers, staff, and stores.
/// </summary>
public class Address
{
    public int AddressId { get; set; }
    public string Address1 { get; set; } = null!;
    public string? Address2 { get; set; }
    public string District { get; set; } = null!;
    public int CityId { get; set; }
    public string? PostalCode { get; set; }
    public string Phone { get; set; } = null!;
    public DateTime LastUpdate { get; set; }

    public City City { get; set; } = null!;
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Staff> Staff { get; set; } = [];
    public ICollection<Store> Stores { get; set; } = [];
}
