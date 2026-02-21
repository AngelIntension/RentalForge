namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a film rental transaction. Links inventory to customer with staff handling.
/// </summary>
public class Rental
{
    public int RentalId { get; set; }
    public DateTime RentalDate { get; set; }
    public int InventoryId { get; set; }
    public int CustomerId { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int StaffId { get; set; }
    public DateTime LastUpdate { get; set; }

    public Inventory Inventory { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = [];
}
