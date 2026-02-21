namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a physical copy of a film at a store. Related to rentals.
/// </summary>
public class Inventory
{
    public int InventoryId { get; set; }
    public int FilmId { get; set; }
    public int StoreId { get; set; }
    public DateTime LastUpdate { get; set; }

    public Film Film { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public ICollection<Rental> Rentals { get; set; } = [];
}
