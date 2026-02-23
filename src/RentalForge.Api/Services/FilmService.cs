using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Models;

namespace RentalForge.Api.Services;

/// <summary>
/// Implements film CRUD operations against the dvdrental database.
/// </summary>
public class FilmService(
    DvdrentalContext db,
    ILogger<FilmService> logger,
    IValidator<CreateFilmRequest> createValidator,
    IValidator<UpdateFilmRequest> updateValidator) : IFilmService
{
    public async Task<PagedResponse<FilmListResponse>> GetFilmsAsync(
        string? search, string? category, string? rating,
        int? yearFrom, int? yearTo, int page, int pageSize)
    {
        logger.LogInformation("Searching films: search={Search}, category={Category}, rating={Rating}, yearFrom={YearFrom}, yearTo={YearTo}, page={Page}, pageSize={PageSize}",
            search, category, rating, yearFrom, yearTo, page, pageSize);

        var query = db.Films.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(f =>
                EF.Functions.ILike(f.Title, pattern) ||
                (f.Description != null && EF.Functions.ILike(f.Description, pattern)) ||
                f.FilmActors.Any(fa =>
                    EF.Functions.ILike(fa.Actor.FirstName, pattern) ||
                    EF.Functions.ILike(fa.Actor.LastName, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(f =>
                f.FilmCategories.Any(fc =>
                    EF.Functions.ILike(fc.Category.Name, category.Trim())));
        }

        if (!string.IsNullOrWhiteSpace(rating) && Enum.TryParse<MpaaRating>(NormalizeRating(rating), true, out var parsedRating))
        {
            query = query.Where(f => f.Rating == parsedRating);
        }

        if (yearFrom.HasValue)
            query = query.Where(f => f.ReleaseYear >= yearFrom.Value);

        if (yearTo.HasValue)
            query = query.Where(f => f.ReleaseYear <= yearTo.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(f => f.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FilmListResponse(
                f.FilmId,
                f.Title,
                f.Description,
                f.ReleaseYear,
                f.LanguageId,
                f.OriginalLanguageId,
                f.RentalDuration,
                f.RentalRate,
                f.Length,
                f.ReplacementCost,
                f.Rating != null ? RatingToString(f.Rating.Value) : null,
                f.SpecialFeatures,
                f.LastUpdate))
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResponse<FilmListResponse>(items, page, pageSize, totalCount, totalPages);
    }

    public async Task<Result<FilmDetailResponse>> GetFilmByIdAsync(int id)
    {
        var film = await db.Films
            .Where(f => f.FilmId == id)
            .Select(f => new FilmDetailResponse(
                f.FilmId,
                f.Title,
                f.Description,
                f.ReleaseYear,
                f.LanguageId,
                f.Language.Name.Trim(),
                f.OriginalLanguageId,
                f.OriginalLanguage != null ? f.OriginalLanguage.Name.Trim() : null,
                f.RentalDuration,
                f.RentalRate,
                f.Length,
                f.ReplacementCost,
                f.Rating != null ? RatingToString(f.Rating.Value) : null,
                f.SpecialFeatures,
                f.LastUpdate,
                f.FilmActors
                    .Select(fa => fa.Actor.FirstName + " " + fa.Actor.LastName)
                    .ToList(),
                f.FilmCategories
                    .Select(fc => fc.Category.Name)
                    .ToList()))
            .FirstOrDefaultAsync();

        return film is not null
            ? Result<FilmDetailResponse>.Success(film)
            : Result<FilmDetailResponse>.NotFound();
    }

    public async Task<Result<FilmDetailResponse>> CreateFilmAsync(CreateFilmRequest request)
    {
        var validationResult = await createValidator.ValidateAsync(request);
        var allErrors = validationResult.AsErrors();

        if (!await db.Languages.AnyAsync(l => l.LanguageId == request.LanguageId))
            allErrors.Add(new ValidationError("languageId", $"Language with ID {request.LanguageId} does not exist."));

        if (request.OriginalLanguageId.HasValue &&
            !await db.Languages.AnyAsync(l => l.LanguageId == request.OriginalLanguageId.Value))
            allErrors.Add(new ValidationError("originalLanguageId", $"Original language with ID {request.OriginalLanguageId.Value} does not exist."));

        if (allErrors.Count > 0)
            return Result<FilmDetailResponse>.Invalid(allErrors);

        var film = new Film
        {
            Title = request.Title,
            Description = request.Description,
            ReleaseYear = request.ReleaseYear,
            LanguageId = request.LanguageId,
            OriginalLanguageId = request.OriginalLanguageId,
            RentalDuration = request.RentalDuration,
            RentalRate = request.RentalRate,
            Length = request.Length,
            ReplacementCost = request.ReplacementCost,
            Rating = request.Rating,
            SpecialFeatures = request.SpecialFeatures,
            LastUpdate = DateTime.UtcNow
        };

        db.Films.Add(film);
        await db.SaveChangesAsync();

        logger.LogInformation("Created film {FilmId} ({Title})", film.FilmId, film.Title);

        var detail = await GetFilmByIdAsync(film.FilmId);
        return Result<FilmDetailResponse>.Created(detail.Value);
    }

    public async Task<Result<FilmDetailResponse>> UpdateFilmAsync(int id, UpdateFilmRequest request)
    {
        var film = await db.Films.FirstOrDefaultAsync(f => f.FilmId == id);

        if (film is null)
            return Result<FilmDetailResponse>.NotFound();

        var validationResult = await updateValidator.ValidateAsync(request);
        var allErrors = validationResult.AsErrors();

        if (!await db.Languages.AnyAsync(l => l.LanguageId == request.LanguageId))
            allErrors.Add(new ValidationError("languageId", $"Language with ID {request.LanguageId} does not exist."));

        if (request.OriginalLanguageId.HasValue &&
            !await db.Languages.AnyAsync(l => l.LanguageId == request.OriginalLanguageId.Value))
            allErrors.Add(new ValidationError("originalLanguageId", $"Original language with ID {request.OriginalLanguageId.Value} does not exist."));

        if (allErrors.Count > 0)
            return Result<FilmDetailResponse>.Invalid(allErrors);

        film.Title = request.Title;
        film.Description = request.Description;
        film.ReleaseYear = request.ReleaseYear;
        film.LanguageId = request.LanguageId;
        film.OriginalLanguageId = request.OriginalLanguageId;
        film.RentalDuration = request.RentalDuration;
        film.RentalRate = request.RentalRate;
        film.Length = request.Length;
        film.ReplacementCost = request.ReplacementCost;
        film.Rating = request.Rating;
        film.SpecialFeatures = request.SpecialFeatures;
        film.LastUpdate = DateTime.UtcNow;

        await db.SaveChangesAsync();

        logger.LogInformation("Updated film {FilmId} ({Title})", film.FilmId, film.Title);

        var detail = await GetFilmByIdAsync(film.FilmId);
        return Result<FilmDetailResponse>.Success(detail.Value);
    }

    public async Task<Result> DeleteFilmAsync(int id)
    {
        var film = await db.Films.FirstOrDefaultAsync(f => f.FilmId == id);

        if (film is null)
            return Result.NotFound();

        if (await db.Inventories.AnyAsync(i => i.FilmId == id))
        {
            logger.LogInformation("Delete blocked for film {FilmId} ({Title}) — has associated inventory records",
                film.FilmId, film.Title);
            return Result.Conflict($"Cannot delete film with ID {id} because it has associated inventory records.");
        }

        var filmActors = await db.FilmActors.Where(fa => fa.FilmId == id).ToListAsync();
        var filmCategories = await db.FilmCategories.Where(fc => fc.FilmId == id).ToListAsync();
        db.FilmActors.RemoveRange(filmActors);
        db.FilmCategories.RemoveRange(filmCategories);
        db.Films.Remove(film);
        await db.SaveChangesAsync();

        logger.LogInformation("Deleted film {FilmId} ({Title})", film.FilmId, film.Title);

        return Result.NoContent();
    }

    private static string RatingToString(MpaaRating rating) => rating switch
    {
        MpaaRating.G => "G",
        MpaaRating.PG => "PG",
        MpaaRating.Pg13 => "PG-13",
        MpaaRating.R => "R",
        MpaaRating.Nc17 => "NC-17",
        _ => rating.ToString()
    };

    private static string NormalizeRating(string rating) => rating.Trim().ToUpperInvariant() switch
    {
        "G" => nameof(MpaaRating.G),
        "PG" => nameof(MpaaRating.PG),
        "PG-13" => nameof(MpaaRating.Pg13),
        "R" => nameof(MpaaRating.R),
        "NC-17" => nameof(MpaaRating.Nc17),
        _ => rating
    };
}
