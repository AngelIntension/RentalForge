namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a language for films. Films have a primary language and optional original language.
/// </summary>
public class Language
{
    public int LanguageId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime LastUpdate { get; set; }

    public ICollection<Film> Films { get; set; } = [];
    public ICollection<Film> FilmsOriginalLanguage { get; set; } = [];
}
