using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalForge.Api.Data;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

public class ReferenceDataTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public ReferenceDataTests(TestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ReferenceData_CountryTable_ContainsExpectedRowCount()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Act
        var count = await context.Countries.CountAsync();

        // Assert
        count.Should().Be(109, "dvdrental has 109 countries as reference data");
    }

    [Fact]
    public async Task ReferenceData_CityTable_ContainsExpectedRowCount()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Act
        var count = await context.Cities.CountAsync();

        // Assert
        count.Should().Be(600, "dvdrental has 600 cities as reference data");
    }

    [Fact]
    public async Task ReferenceData_LanguageTable_ContainsExpectedRowCount()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Act
        var count = await context.Languages.CountAsync();

        // Assert
        count.Should().Be(6, "dvdrental has 6 languages as reference data");
    }

    [Fact]
    public async Task ReferenceData_CategoryTable_ContainsExpectedRowCount()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Act
        var count = await context.Categories.CountAsync();

        // Assert
        count.Should().Be(16, "dvdrental has 16 categories as reference data");
    }

    [Fact]
    public async Task ReferenceData_CitiesHaveValidCountryAssociations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Act — get all city CountryIds and all valid country IDs
        var cityCountryIds = await context.Cities
            .Select(c => c.CountryId)
            .Distinct()
            .ToListAsync();

        var validCountryIds = await context.Countries
            .Select(c => c.CountryId)
            .ToListAsync();

        // Assert — every city's CountryId should reference an existing country
        cityCountryIds.Should().OnlyContain(
            id => validCountryIds.Contains(id),
            "every city must reference a valid country");
    }

    [Fact]
    public async Task ReferenceData_NonReferenceTables_AreEmpty()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Act — count rows in all 11 non-reference tables
        var actorCount = await context.Actors.CountAsync();
        var addressCount = await context.Addresses.CountAsync();
        var customerCount = await context.Customers.CountAsync();
        var filmCount = await context.Films.CountAsync();
        var filmActorCount = await context.FilmActors.CountAsync();
        var filmCategoryCount = await context.FilmCategories.CountAsync();
        var inventoryCount = await context.Inventories.CountAsync();
        var paymentCount = await context.Payments.CountAsync();
        var rentalCount = await context.Rentals.CountAsync();
        var staffCount = await context.Staff.CountAsync();
        var storeCount = await context.Stores.CountAsync();

        // Assert — all non-reference tables should be empty
        actorCount.Should().Be(0, "actor is a non-reference table and should be empty after schema creation");
        addressCount.Should().Be(0, "address is a non-reference table and should be empty after schema creation");
        customerCount.Should().Be(0, "customer is a non-reference table and should be empty after schema creation");
        filmCount.Should().Be(0, "film is a non-reference table and should be empty after schema creation");
        filmActorCount.Should().Be(0, "film_actor is a non-reference table and should be empty after schema creation");
        filmCategoryCount.Should().Be(0, "film_category is a non-reference table and should be empty after schema creation");
        inventoryCount.Should().Be(0, "inventory is a non-reference table and should be empty after schema creation");
        paymentCount.Should().Be(0, "payment is a non-reference table and should be empty after schema creation");
        rentalCount.Should().Be(0, "rental is a non-reference table and should be empty after schema creation");
        staffCount.Should().Be(0, "staff is a non-reference table and should be empty after schema creation");
        storeCount.Should().Be(0, "store is a non-reference table and should be empty after schema creation");
    }

    [Fact]
    public async Task ReferenceData_EnsureCreatedTwice_DoesNotDuplicateData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Act — call EnsureCreatedAsync again (it was already called in TestWebAppFactory)
        await context.Database.EnsureCreatedAsync();

        // Assert — reference data should not be duplicated
        (await context.Countries.CountAsync()).Should().Be(109, "EnsureCreatedAsync twice should not duplicate countries");
        (await context.Cities.CountAsync()).Should().Be(600, "EnsureCreatedAsync twice should not duplicate cities");
        (await context.Languages.CountAsync()).Should().Be(6, "EnsureCreatedAsync twice should not duplicate languages");
        (await context.Categories.CountAsync()).Should().Be(16, "EnsureCreatedAsync twice should not duplicate categories");
    }
}
