namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a film category (e.g., Action, Comedy). Related to films through the FilmCategory join table.
/// </summary>
public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime LastUpdate { get; set; }

    public ICollection<FilmCategory> FilmCategories { get; set; } = [];
}
