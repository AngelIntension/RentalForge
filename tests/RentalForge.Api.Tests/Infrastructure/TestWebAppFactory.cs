using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using Testcontainers.PostgreSql;

namespace RentalForge.Api.Tests.Infrastructure;

public class TestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestJwtKey = "ThisIsATestSigningKeyThatIsAtLeast256BitsLong!!";
    public const string TestJwtIssuer = "RentalForge.Tests";
    public const string TestJwtAudience = "RentalForge.Tests";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Create the schema from the EF Core model
        var options = new DbContextOptionsBuilder<DvdrentalContext>()
            .UseNpgsql(_postgres.GetConnectionString(),
                o => o.MapEnum<MpaaRating>("mpaa_rating"))
            .Options;

        await using var context = new DvdrentalContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override the connection string — Program.cs reads from config and configures
        // UseNpgsql(connectionString, o => o.MapEnum<MpaaRating>()) which handles everything.
        builder.UseSetting("ConnectionStrings:Dvdrental", _postgres.GetConnectionString());

        // JWT configuration for test environment
        builder.UseSetting("Jwt:Key", TestJwtKey);
        builder.UseSetting("Jwt:Issuer", TestJwtIssuer);
        builder.UseSetting("Jwt:Audience", TestJwtAudience);
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("Jwt:RefreshTokenExpirationDays", "7");

        // Override rate limits with very permissive values so non-rate-limit tests aren't throttled
        builder.UseSetting("RateLimiting:LoginPermitLimit", "10000");
        builder.UseSetting("RateLimiting:RegisterPermitLimit", "10000");
        builder.UseSetting("RateLimiting:RefreshPermitLimit", "10000");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
