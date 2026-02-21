using NpgsqlTypes;

namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a film in the catalog. Related to actors and categories through join tables.
/// </summary>
public class Film
{
    public int FilmId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int LanguageId { get; set; }
    public int? OriginalLanguageId { get; set; }
    public short RentalDuration { get; set; }
    public decimal RentalRate { get; set; }
    public short? Length { get; set; }
    public decimal ReplacementCost { get; set; }
    public MpaaRating? Rating { get; set; }
    public DateTime LastUpdate { get; set; }
    public string[]? SpecialFeatures { get; set; }
    public NpgsqlTsVector Fulltext { get; set; } = null!;

    public Language Language { get; set; } = null!;
    public Language? OriginalLanguage { get; set; }
    public ICollection<FilmActor> FilmActors { get; set; } = [];
    public ICollection<FilmCategory> FilmCategories { get; set; } = [];
    public ICollection<Inventory> Inventories { get; set; } = [];
}
