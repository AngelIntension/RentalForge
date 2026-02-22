namespace RentalForge.Api.Models;

/// <summary>
/// Response DTO for the /health endpoint (Principle VI: immutable record).
/// </summary>
public record HealthResponse(
    string Status,
    string? DatabaseVersion = null,
    DateTimeOffset? ServerTime = null,
    string? Error = null);
