using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Seeding;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

public class DevDataSeederTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public DevDataSeederTests(TestWebAppFactory factory)
    {
        _factory = factory;
    }

    private (DvdrentalContext context, DevDataSeeder seeder) CreateSeeder()
    {
        var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DevDataSeeder>>();
        return (context, new DevDataSeeder(context, logger));
    }

    private async Task EnsureCleanStateAsync(DvdrentalContext context)
    {
        // Truncate all non-reference tables to ensure test isolation
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE actor, address, customer, film, film_actor, film_category, inventory, payment, rental, staff, store CASCADE");
        context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task Seed_PopulatesAllNonReferenceTables()
    {
        // Arrange
        var (context, seeder) = CreateSeeder();
        await EnsureCleanStateAsync(context);

        // Act
        var result = await seeder.SeedAsync();

        // Assert
        result.Should().BeTrue("seeding into empty tables should succeed");

        (await context.Actors.CountAsync()).Should().Be(200);
        (await context.Addresses.CountAsync()).Should().Be(603);
        (await context.Films.CountAsync()).Should().Be(1000);
        (await context.Staff.CountAsync()).Should().Be(2);
        (await context.Stores.CountAsync()).Should().Be(2);
        (await context.Customers.CountAsync()).Should().Be(599);
        (await context.FilmActors.CountAsync()).Should().Be(5462);
        (await context.FilmCategories.CountAsync()).Should().Be(1000);
        (await context.Inventories.CountAsync()).Should().Be(4581);
        (await context.Rentals.CountAsync()).Should().Be(16044);
        (await context.Payments.CountAsync()).Should().Be(14596);
    }

    [Fact]
    public async Task Seed_PreservesReferenceData()
    {
        // Arrange
        var (context, seeder) = CreateSeeder();
        await EnsureCleanStateAsync(context);

        // Act
        await seeder.SeedAsync();

        // Assert — reference data should be unchanged
        (await context.Countries.CountAsync()).Should().Be(109);
        (await context.Cities.CountAsync()).Should().Be(600);
        (await context.Languages.CountAsync()).Should().Be(6);
        (await context.Categories.CountAsync()).Should().Be(16);
    }

    [Fact]
    public async Task Seed_MaintainsForeignKeyIntegrity()
    {
        // Arrange
        var (context, seeder) = CreateSeeder();
        await EnsureCleanStateAsync(context);

        // Act
        await seeder.SeedAsync();

        // Assert — spot check FK integrity
        var filmsWithInvalidLanguage = await context.Films
            .Where(f => !context.Languages.Any(l => l.LanguageId == f.LanguageId))
            .CountAsync();
        filmsWithInvalidLanguage.Should().Be(0, "all films must reference a valid language");

        var rentalsWithInvalidCustomer = await context.Rentals
            .Where(r => !context.Customers.Any(c => c.CustomerId == r.CustomerId))
            .CountAsync();
        rentalsWithInvalidCustomer.Should().Be(0, "all rentals must reference a valid customer");
    }

    [Fact]
    public async Task Seed_SkipsWhenDataExists()
    {
        // Arrange
        var (context, seeder) = CreateSeeder();
        await EnsureCleanStateAsync(context);
        await seeder.SeedAsync(); // First seed

        // Act
        var result = await seeder.SeedAsync(); // Second seed

        // Assert
        result.Should().BeFalse("seeding should be skipped when data already exists");
        (await context.Actors.CountAsync()).Should().Be(200, "data should remain unchanged");
    }

    [Fact]
    public async Task SeedForce_ClearsAndReseeds()
    {
        // Arrange
        var (context, seeder) = CreateSeeder();
        await EnsureCleanStateAsync(context);
        await seeder.SeedAsync(); // Initial seed

        // Act
        await seeder.SeedForceAsync();

        // Assert — all non-reference tables should have correct counts
        (await context.Actors.CountAsync()).Should().Be(200);
        (await context.Addresses.CountAsync()).Should().Be(603);
        (await context.Films.CountAsync()).Should().Be(1000);
        (await context.Staff.CountAsync()).Should().Be(2);
        (await context.Stores.CountAsync()).Should().Be(2);
        (await context.Customers.CountAsync()).Should().Be(599);
        (await context.FilmActors.CountAsync()).Should().Be(5462);
        (await context.FilmCategories.CountAsync()).Should().Be(1000);
        (await context.Inventories.CountAsync()).Should().Be(4581);
        (await context.Rentals.CountAsync()).Should().Be(16044);
        (await context.Payments.CountAsync()).Should().Be(14596);
    }

    [Fact]
    public async Task SeedForce_PreservesReferenceData()
    {
        // Arrange
        var (context, seeder) = CreateSeeder();
        await EnsureCleanStateAsync(context);
        await seeder.SeedAsync(); // Initial seed

        // Act
        await seeder.SeedForceAsync();

        // Assert — reference data preserved
        (await context.Countries.CountAsync()).Should().Be(109);
        (await context.Cities.CountAsync()).Should().Be(600);
        (await context.Languages.CountAsync()).Should().Be(6);
        (await context.Categories.CountAsync()).Should().Be(16);
    }

    [Fact]
    public async Task SeedForce_RunTwice_DataCorrectAfterEach()
    {
        // Arrange
        var (context, seeder) = CreateSeeder();
        await EnsureCleanStateAsync(context);
        await seeder.SeedAsync(); // Initial seed

        // Act — force re-seed twice
        await seeder.SeedForceAsync();
        await seeder.SeedForceAsync();

        // Assert — data correct after second force re-seed
        (await context.Actors.CountAsync()).Should().Be(200);
        (await context.Films.CountAsync()).Should().Be(1000);
        (await context.Rentals.CountAsync()).Should().Be(16044);
        (await context.Payments.CountAsync()).Should().Be(14596);
    }

    [Fact]
    public async Task Seed_OnPartiallySeededDatabase_SkipsGracefully()
    {
        // Arrange — insert only actors, simulating partial state
        var (context, seeder) = CreateSeeder();
        await EnsureCleanStateAsync(context);

        // Manually insert a single actor to simulate partial seed
        context.Actors.Add(new RentalForge.Api.Data.Entities.Actor
        {
            ActorId = 999,
            FirstName = "Test",
            LastName = "Partial",
            LastUpdate = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        // Act — SeedAsync should detect existing data and skip
        var result = await seeder.SeedAsync();

        // Assert
        result.Should().BeFalse("seeder should detect existing actor data and skip");
    }

    [Fact]
    public async Task SeedForce_OnPartiallySeededDatabase_CleansAndReseeds()
    {
        // Arrange — create partial state
        var (context, seeder) = CreateSeeder();
        await EnsureCleanStateAsync(context);

        context.Actors.Add(new RentalForge.Api.Data.Entities.Actor
        {
            ActorId = 999,
            FirstName = "Test",
            LastName = "Partial",
            LastUpdate = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        // Act — force re-seed should clean and reseed correctly
        await seeder.SeedForceAsync();

        // Assert — correct counts (the partial actor should be gone)
        (await context.Actors.CountAsync()).Should().Be(200);
        (await context.Films.CountAsync()).Should().Be(1000);
        (await context.Rentals.CountAsync()).Should().Be(16044);
    }
}
