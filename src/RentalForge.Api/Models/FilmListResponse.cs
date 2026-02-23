using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Models;

/// <summary>
/// Lean response DTO for film list items. Returns IDs for related entities per constitution v1.8.0.
/// </summary>
public record FilmListResponse(
    int Id,
    string Title,
    string? Description,
    int? ReleaseYear,
    int LanguageId,
    int? OriginalLanguageId,
    short RentalDuration,
    decimal RentalRate,
    short? Length,
    decimal ReplacementCost,
    MpaaRating? Rating,
    string[]? SpecialFeatures,
    DateTime LastUpdate);
