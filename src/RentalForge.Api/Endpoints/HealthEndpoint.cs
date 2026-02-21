using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;

namespace RentalForge.Api.Endpoints;

/// <summary>
/// Response DTO for the /health endpoint (Principle VI: immutable record).
/// </summary>
public record HealthResponse(
    string Status,
    string? DatabaseVersion = null,
    DateTimeOffset? ServerTime = null,
    string? Error = null);

public static class HealthEndpoint
{
    public static void MapHealthEndpoint(this WebApplication app)
    {
        app.MapGet("/health", async (DvdrentalContext db, ILogger<DvdrentalContext> logger) =>
        {
            try
            {
                DbConnection connection = db.Database.GetDbConnection();
                await connection.OpenAsync();

                string? version = null;
                DateTimeOffset? serverTime = null;

                await using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT version()";
                    var result = await cmd.ExecuteScalarAsync();
                    version = result?.ToString();
                }

                await using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT NOW()";
                    var result = await cmd.ExecuteScalarAsync();
                    if (result is DateTime dt)
                        serverTime = new DateTimeOffset(dt, TimeSpan.Zero);
                    else if (result is DateTimeOffset dto)
                        serverTime = dto;
                }

                return Results.Ok(new HealthResponse(
                    Status: "healthy",
                    DatabaseVersion: version,
                    ServerTime: serverTime));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Health check failed: database connection error");

                return Results.Json(
                    new HealthResponse(
                        Status: "unhealthy",
                        Error: $"Database connection failed: {ex.Message}"),
                    statusCode: 503);
            }
        })
        .WithName("HealthCheck")
        .WithSummary("Database health check")
        .Produces<HealthResponse>(StatusCodes.Status200OK, "application/json")
        .Produces<HealthResponse>(StatusCodes.Status503ServiceUnavailable, "application/json")
        .WithOpenApi(operation =>
        {
            operation.Responses["200"].Description = "Database is healthy and reachable";
            operation.Responses["503"].Description = "Database is unhealthy or unreachable";
            return operation;
        });
    }
}
