using Ardalis.Result;
using RentalForge.Api.Models;

namespace RentalForge.Api.Services;

/// <summary>
/// Defines operations for managing film entities.
/// </summary>
public interface IFilmService
{
    Task<PagedResponse<FilmListResponse>> GetFilmsAsync(
        string? search, string? category, string? rating,
        int? yearFrom, int? yearTo, int page, int pageSize);

    Task<Result<FilmDetailResponse>> GetFilmByIdAsync(int id);
    Task<Result<FilmDetailResponse>> CreateFilmAsync(CreateFilmRequest request);
    Task<Result<FilmDetailResponse>> UpdateFilmAsync(int id, UpdateFilmRequest request);
    Task<Result> DeleteFilmAsync(int id);
}
