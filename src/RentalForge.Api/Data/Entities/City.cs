namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a city within a country. Contains addresses.
/// </summary>
public class City
{
    public int CityId { get; set; }
    public string CityName { get; set; } = null!;
    public int CountryId { get; set; }
    public DateTime LastUpdate { get; set; }

    public Country Country { get; set; } = null!;
    public ICollection<Address> Addresses { get; set; } = [];
}
