using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Tests.Infrastructure;

/// <summary>
/// Seeds test data for film integration tests.
/// Uses high ID range (9000+) to avoid collisions with reference data.
/// </summary>
public static class FilmTestHelper
{
    /// <summary>
    /// Seeds languages, actors, categories, films with varied ratings/years,
    /// and film_actor/film_category join rows for integration tests.
    /// </summary>
    public static async Task SeedTestDataAsync(DvdrentalContext db)
    {
        // Languages (use high IDs to avoid collisions with HasData seeds)
        var languages = new List<Language>
        {
            new() { LanguageId = 9001, Name = "English             ", LastUpdate = DateTime.UtcNow },
            new() { LanguageId = 9002, Name = "French              ", LastUpdate = DateTime.UtcNow }
        };
        db.Languages.AddRange(languages);
        await db.SaveChangesAsync();

        // Actors
        var actors = new List<Actor>
        {
            new() { ActorId = 9001, FirstName = "John", LastName = "Smith", LastUpdate = DateTime.UtcNow },
            new() { ActorId = 9002, FirstName = "Jane", LastName = "Doe", LastUpdate = DateTime.UtcNow },
            new() { ActorId = 9003, FirstName = "Bob", LastName = "Wilson", LastUpdate = DateTime.UtcNow }
        };
        db.Actors.AddRange(actors);
        await db.SaveChangesAsync();

        // Categories
        var categories = new List<Category>
        {
            new() { CategoryId = 9001, Name = "Action", LastUpdate = DateTime.UtcNow },
            new() { CategoryId = 9002, Name = "Comedy", LastUpdate = DateTime.UtcNow },
            new() { CategoryId = 9003, Name = "Drama", LastUpdate = DateTime.UtcNow }
        };
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        // Films with varied ratings and release years
        var films = new List<Film>
        {
            new()
            {
                FilmId = 9001,
                Title = "Alpha Adventure",
                Description = "An exciting action adventure film",
                ReleaseYear = 2020,
                LanguageId = 9001,
                OriginalLanguageId = null,
                RentalDuration = 5,
                RentalRate = 3.99m,
                Length = 120,
                ReplacementCost = 24.99m,
                Rating = MpaaRating.PG,
                SpecialFeatures = ["Trailers", "Behind the Scenes"],
                LastUpdate = DateTime.UtcNow
            },
            new()
            {
                FilmId = 9002,
                Title = "Beta Bonanza",
                Description = "A hilarious comedy bonanza",
                ReleaseYear = 2018,
                LanguageId = 9001,
                OriginalLanguageId = 9002,
                RentalDuration = 3,
                RentalRate = 2.99m,
                Length = 90,
                ReplacementCost = 19.99m,
                Rating = MpaaRating.R,
                SpecialFeatures = ["Deleted Scenes"],
                LastUpdate = DateTime.UtcNow
            },
            new()
            {
                FilmId = 9003,
                Title = "Charlie Chase",
                Description = "A dramatic chase through the city",
                ReleaseYear = 2022,
                LanguageId = 9001,
                OriginalLanguageId = null,
                RentalDuration = 7,
                RentalRate = 4.99m,
                Length = 150,
                ReplacementCost = 29.99m,
                Rating = MpaaRating.Pg13,
                SpecialFeatures = null,
                LastUpdate = DateTime.UtcNow
            },
            new()
            {
                FilmId = 9004,
                Title = "Delta Drama",
                Description = "An intense love drama",
                ReleaseYear = 2015,
                LanguageId = 9002,
                OriginalLanguageId = null,
                RentalDuration = 4,
                RentalRate = 1.99m,
                Length = null,
                ReplacementCost = 14.99m,
                Rating = MpaaRating.G,
                SpecialFeatures = null,
                LastUpdate = DateTime.UtcNow
            },
            new()
            {
                FilmId = 9005,
                Title = "Echo Explosion",
                Description = null,
                ReleaseYear = 2023,
                LanguageId = 9001,
                OriginalLanguageId = null,
                RentalDuration = 6,
                RentalRate = 5.99m,
                Length = 180,
                ReplacementCost = 34.99m,
                Rating = MpaaRating.Nc17,
                SpecialFeatures = ["Commentaries"],
                LastUpdate = DateTime.UtcNow
            }
        };
        db.Films.AddRange(films);
        await db.SaveChangesAsync();

        // Film-Actor join rows
        var filmActors = new List<FilmActor>
        {
            new() { FilmId = 9001, ActorId = 9001, LastUpdate = DateTime.UtcNow },
            new() { FilmId = 9001, ActorId = 9002, LastUpdate = DateTime.UtcNow },
            new() { FilmId = 9002, ActorId = 9002, LastUpdate = DateTime.UtcNow },
            new() { FilmId = 9002, ActorId = 9003, LastUpdate = DateTime.UtcNow },
            new() { FilmId = 9003, ActorId = 9001, LastUpdate = DateTime.UtcNow },
            new() { FilmId = 9004, ActorId = 9003, LastUpdate = DateTime.UtcNow },
            new() { FilmId = 9005, ActorId = 9001, LastUpdate = DateTime.UtcNow },
            new() { FilmId = 9005, ActorId = 9002, LastUpdate = DateTime.UtcNow },
            new() { FilmId = 9005, ActorId = 9003, LastUpdate = DateTime.UtcNow }
        };
        db.FilmActors.AddRange(filmActors);
        await db.SaveChangesAsync();

        // Film-Category join rows
        var filmCategories = new List<FilmCategory>
        {
            new() { FilmId = 9001, CategoryId = 9001, LastUpdate = DateTime.UtcNow }, // Alpha -> Action
            new() { FilmId = 9002, CategoryId = 9002, LastUpdate = DateTime.UtcNow }, // Beta -> Comedy
            new() { FilmId = 9003, CategoryId = 9001, LastUpdate = DateTime.UtcNow }, // Charlie -> Action
            new() { FilmId = 9003, CategoryId = 9003, LastUpdate = DateTime.UtcNow }, // Charlie -> Drama
            new() { FilmId = 9004, CategoryId = 9003, LastUpdate = DateTime.UtcNow }, // Delta -> Drama
            new() { FilmId = 9005, CategoryId = 9001, LastUpdate = DateTime.UtcNow }  // Echo -> Action
        };
        db.FilmCategories.AddRange(filmCategories);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a film with associated inventory records for delete-blocking tests.
    /// </summary>
    public static async Task SeedFilmWithInventoryAsync(DvdrentalContext db)
    {
        // Need a store for inventory - use raw SQL for the circular FK
        await db.Database.ExecuteSqlRawAsync(
            """
            SET session_replication_role = 'replica';

            INSERT INTO address (address_id, address, district, city_id, phone, last_update)
            VALUES (9501, '500 Film Store St', 'TestDistrict', 1, '555-9501', NOW())
            ON CONFLICT DO NOTHING;

            INSERT INTO store (store_id, manager_staff_id, address_id, last_update)
            VALUES (9501, 9501, 9501, NOW())
            ON CONFLICT DO NOTHING;

            INSERT INTO staff (staff_id, first_name, last_name, address_id, store_id, active, username, last_update)
            VALUES (9501, 'Film', 'Manager', 9501, 9501, true, 'filmmgr', NOW())
            ON CONFLICT DO NOTHING;

            SET session_replication_role = 'origin';
            """);

        // Create a film specifically for inventory-blocking test
        var film = new Film
        {
            FilmId = 9100,
            Title = "Inventory Film",
            Description = "A film with inventory records",
            ReleaseYear = 2020,
            LanguageId = 9001,
            RentalDuration = 5,
            RentalRate = 3.99m,
            ReplacementCost = 24.99m,
            Rating = MpaaRating.PG,
            LastUpdate = DateTime.UtcNow
        };
        db.Films.Add(film);
        await db.SaveChangesAsync();

        // Add inventory record to block deletion
        var inventory = new Inventory
        {
            InventoryId = 9001,
            FilmId = 9100,
            StoreId = 9501,
            LastUpdate = DateTime.UtcNow
        };
        db.Inventories.Add(inventory);
        await db.SaveChangesAsync();
    }
}
