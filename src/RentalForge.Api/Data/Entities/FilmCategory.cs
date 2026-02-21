namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Join table linking films to categories. Composite PK on (FilmId, CategoryId).
/// </summary>
public class FilmCategory
{
    public int FilmId { get; set; }
    public int CategoryId { get; set; }
    public DateTime LastUpdate { get; set; }

    public Film Film { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
