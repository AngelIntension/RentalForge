using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using Testcontainers.PostgreSql;

namespace RentalForge.Api.Tests.Infrastructure;

public class TestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18")
        .Build();

    private NpgsqlDataSource? _dataSource;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Build data source and create the schema from the EF Core model
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_postgres.GetConnectionString());
        dataSourceBuilder.MapEnum<MpaaRating>("mpaa_rating");
        _dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<DvdrentalContext>()
            .UseNpgsql(_dataSource)
            .Options;

        await using var context = new DvdrentalContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing registrations
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<DvdrentalContext>)
                         || d.ServiceType == typeof(NpgsqlDataSource))
                .ToList();
            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            services.AddDbContext<DvdrentalContext>(options =>
                options.UseNpgsql(_dataSource!));
        });

        // Override the connection string in configuration so Program.cs validation passes
        builder.UseSetting("ConnectionStrings:Dvdrental", _postgres.GetConnectionString());
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
