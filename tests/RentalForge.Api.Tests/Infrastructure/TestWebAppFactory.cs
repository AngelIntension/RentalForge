using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using Testcontainers.PostgreSql;

namespace RentalForge.Api.Tests.Infrastructure;

public class TestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
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
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
