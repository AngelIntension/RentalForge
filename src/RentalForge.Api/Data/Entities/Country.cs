namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a country. Contains cities.
/// </summary>
public class Country
{
    public int CountryId { get; set; }
    public string CountryName { get; set; } = null!;
    public DateTime LastUpdate { get; set; }

    public ICollection<City> Cities { get; set; } = [];
}
