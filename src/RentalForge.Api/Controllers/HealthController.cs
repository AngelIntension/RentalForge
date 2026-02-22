using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace RentalForge.Api.Controllers;

/// <summary>
/// Provides a database health check endpoint for operational monitoring.
/// </summary>
[ApiController]
[Route("")]
public class HealthController(DvdrentalContext db, ILogger<HealthController> logger) : ControllerBase
{
    /// <summary>
    /// Checks database connectivity and returns server version and time.
    /// </summary>
    /// <returns>A <see cref="HealthResponse"/> indicating database status.</returns>
    [HttpGet("health")]
    [SwaggerOperation(OperationId = "HealthCheck", Summary = "Database health check")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    [SwaggerResponse(StatusCodes.Status200OK, "Database is healthy and reachable", typeof(HealthResponse))]
    [SwaggerResponse(StatusCodes.Status503ServiceUnavailable, "Database is unhealthy or unreachable", typeof(HealthResponse))]
    public async Task<IActionResult> CheckHealthAsync()
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

            return Ok(new HealthResponse(
                Status: "healthy",
                DatabaseVersion: version,
                ServerTime: serverTime));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed: database connection error");

            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new HealthResponse(
                    Status: "unhealthy",
                    Error: $"Database connection failed: {ex.Message}"));
        }
    }
}
