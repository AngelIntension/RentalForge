using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using Testcontainers.PostgreSql;

namespace RentalForge.Api.Tests.Infrastructure;

/// <summary>
/// Test factory that preserves production rate limit configuration for rate limit tests.
/// Unlike TestWebAppFactory, this does NOT override rate limiters with permissive values.
/// </summary>
public class RateLimitTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<DvdrentalContext>()
            .UseNpgsql(_postgres.GetConnectionString(),
                o => o.MapEnum<MpaaRating>("mpaa_rating"))
            .Options;

        await using var context = new DvdrentalContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Dvdrental", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Key", TestWebAppFactory.TestJwtKey);
        builder.UseSetting("Jwt:Issuer", TestWebAppFactory.TestJwtIssuer);
        builder.UseSetting("Jwt:Audience", TestWebAppFactory.TestJwtAudience);
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("Jwt:RefreshTokenExpirationDays", "7");
        // No rate limit override — keeps production limits (3/min register, 5/min login, 10/min refresh)
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
