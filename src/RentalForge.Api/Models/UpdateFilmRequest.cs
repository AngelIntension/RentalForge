using System.Text.Json.Serialization;
using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Models;

/// <summary>
/// Request DTO for updating an existing film.
/// </summary>
public record UpdateFilmRequest
{
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public int? ReleaseYear { get; init; }
    public int LanguageId { get; init; }
    public int? OriginalLanguageId { get; init; }
    public short RentalDuration { get; init; }
    public decimal RentalRate { get; init; }
    public short? Length { get; init; }
    public decimal ReplacementCost { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MpaaRating? Rating { get; init; }

    public string[]? SpecialFeatures { get; init; }
}
