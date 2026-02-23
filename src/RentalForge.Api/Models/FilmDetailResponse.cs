using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Models;

/// <summary>
/// Rich response DTO for film detail. Includes flat related data per constitution v1.8.0.
/// </summary>
public record FilmDetailResponse(
    int Id,
    string Title,
    string? Description,
    int? ReleaseYear,
    int LanguageId,
    string LanguageName,
    int? OriginalLanguageId,
    string? OriginalLanguageName,
    short RentalDuration,
    decimal RentalRate,
    short? Length,
    decimal ReplacementCost,
    MpaaRating? Rating,
    string[]? SpecialFeatures,
    DateTime LastUpdate,
    IReadOnlyList<string> Actors,
    IReadOnlyList<string> Categories);
