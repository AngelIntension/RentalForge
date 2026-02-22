namespace RentalForge.Api.Models;

/// <summary>
/// Generic wrapper for paginated API responses.
/// </summary>
public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
