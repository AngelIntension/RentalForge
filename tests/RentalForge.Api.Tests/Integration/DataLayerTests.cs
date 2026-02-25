using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalForge.Api.Data;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

public class DataLayerTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public DataLayerTests(TestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void DbContext_RegistersAllDbSets()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Act — get all DbSet<> properties declared on DvdrentalContext (not inherited Identity sets)
        var dbSetProperties = context.GetType()
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                        && p.DeclaringType == typeof(DvdrentalContext))
            .ToList();

        // Assert — 15 dvdrental tables + 1 RefreshToken = 16
        dbSetProperties.Should().HaveCount(16,
            "dvdrental has 15 tables (actor, address, category, city, country, customer, " +
            "film, film_actor, film_category, inventory, language, payment, rental, staff, store) " +
            "plus 1 identity table (refresh_tokens)");
    }

    [Fact]
    public async Task DbContext_CanQueryFilmTable()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Act — query the films table (property name determined after scaffold)
        var act = async () => await context.Films.Take(1).ToListAsync();

        // Assert — no exception thrown means entity mapping is correct
        await act.Should().NotThrowAsync();
    }
}
