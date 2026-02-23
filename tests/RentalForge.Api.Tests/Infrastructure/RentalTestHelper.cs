using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Tests.Infrastructure;

/// <summary>
/// Seeds test data for rental integration tests.
/// Uses high ID range (9000+) to avoid collisions with reference data.
/// </summary>
public static class RentalTestHelper
{
    /// <summary>
    /// Seeds a complete set of related entities for rental integration tests:
    /// 2 stores, 2 staff (1 active + 1 inactive), 3 customers (2 active + 1 inactive),
    /// 2 films, 1 language, inventory records, rental records (active + returned).
    /// </summary>
    public static async Task SeedTestDataAsync(DvdrentalContext db)
    {
        // Addresses for stores, staff, and customers
        var addresses = new List<Address>
        {
            new() { AddressId = 9001, Address1 = "100 Rental Store St", District = "TestDistrict", CityId = 1, Phone = "555-9001", LastUpdate = DateTime.UtcNow },
            new() { AddressId = 9002, Address1 = "101 Rental Store St", District = "TestDistrict", CityId = 1, Phone = "555-9002", LastUpdate = DateTime.UtcNow },
            new() { AddressId = 9003, Address1 = "200 Staff Ave", District = "TestDistrict", CityId = 1, Phone = "555-9003", LastUpdate = DateTime.UtcNow },
            new() { AddressId = 9004, Address1 = "201 Staff Ave", District = "TestDistrict", CityId = 1, Phone = "555-9004", LastUpdate = DateTime.UtcNow },
            new() { AddressId = 9005, Address1 = "300 Customer Ln", District = "TestDistrict", CityId = 1, Phone = "555-9005", LastUpdate = DateTime.UtcNow },
            new() { AddressId = 9006, Address1 = "301 Customer Ln", District = "TestDistrict", CityId = 1, Phone = "555-9006", LastUpdate = DateTime.UtcNow },
            new() { AddressId = 9007, Address1 = "302 Customer Ln", District = "TestDistrict", CityId = 1, Phone = "555-9007", LastUpdate = DateTime.UtcNow },
        };
        db.Addresses.AddRange(addresses);
        await db.SaveChangesAsync();

        // Stores and Staff via raw SQL for circular FK
        await db.Database.ExecuteSqlRawAsync(
            """
            SET session_replication_role = 'replica';

            INSERT INTO store (store_id, manager_staff_id, address_id, last_update)
            VALUES (9001, 9001, 9001, NOW()),
                   (9002, 9002, 9002, NOW());

            INSERT INTO staff (staff_id, first_name, last_name, address_id, store_id, active, username, last_update)
            VALUES (9001, 'Active', 'Staffer', 9003, 9001, true, 'activestaff', NOW()),
                   (9002, 'Inactive', 'Staffer', 9004, 9002, false, 'inactivestaff', NOW());

            SET session_replication_role = 'origin';
            """);

        // Customers: 2 active + 1 inactive
        var customers = new List<Customer>
        {
            new()
            {
                CustomerId = 9001, StoreId = 9001, FirstName = "Alice", LastName = "Anderson",
                Email = "alice@rental.test", AddressId = 9005, Activebool = true, Active = 1,
                CreateDate = new DateOnly(2024, 1, 1), LastUpdate = DateTime.UtcNow
            },
            new()
            {
                CustomerId = 9002, StoreId = 9001, FirstName = "Bob", LastName = "Brown",
                Email = "bob@rental.test", AddressId = 9006, Activebool = true, Active = 1,
                CreateDate = new DateOnly(2024, 1, 2), LastUpdate = DateTime.UtcNow
            },
            new()
            {
                CustomerId = 9003, StoreId = 9001, FirstName = "Charlie", LastName = "Clark",
                Email = "charlie@rental.test", AddressId = 9007, Activebool = false, Active = 0,
                CreateDate = new DateOnly(2024, 1, 3), LastUpdate = DateTime.UtcNow
            },
        };
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();

        // Language + Films
        var language = new Language { LanguageId = 9001, Name = "English             ", LastUpdate = DateTime.UtcNow };
        db.Languages.Add(language);
        await db.SaveChangesAsync();

        var films = new List<Film>
        {
            new()
            {
                FilmId = 9001, Title = "Rental Test Film A", Description = "First test film",
                ReleaseYear = 2020, LanguageId = 9001, RentalDuration = 5, RentalRate = 3.99m,
                Length = 120, ReplacementCost = 24.99m, Rating = MpaaRating.PG, LastUpdate = DateTime.UtcNow
            },
            new()
            {
                FilmId = 9002, Title = "Rental Test Film B", Description = "Second test film",
                ReleaseYear = 2022, LanguageId = 9001, RentalDuration = 3, RentalRate = 2.99m,
                Length = 90, ReplacementCost = 19.99m, Rating = MpaaRating.R, LastUpdate = DateTime.UtcNow
            },
        };
        db.Films.AddRange(films);
        await db.SaveChangesAsync();

        // Inventory: Film A at store 1 (3 copies), Film A at store 2 (1 copy), Film B at store 1 (2 copies)
        var inventories = new List<Inventory>
        {
            new() { InventoryId = 9001, FilmId = 9001, StoreId = 9001, LastUpdate = DateTime.UtcNow },
            new() { InventoryId = 9002, FilmId = 9001, StoreId = 9001, LastUpdate = DateTime.UtcNow },
            new() { InventoryId = 9003, FilmId = 9001, StoreId = 9001, LastUpdate = DateTime.UtcNow },
            new() { InventoryId = 9004, FilmId = 9001, StoreId = 9002, LastUpdate = DateTime.UtcNow },
            new() { InventoryId = 9005, FilmId = 9002, StoreId = 9001, LastUpdate = DateTime.UtcNow },
            new() { InventoryId = 9006, FilmId = 9002, StoreId = 9001, LastUpdate = DateTime.UtcNow },
        };
        db.Inventories.AddRange(inventories);
        await db.SaveChangesAsync();

        // Rentals: mix of active and returned
        var rentals = new List<Rental>
        {
            // Active rental: Film A, store 1, inventory 9001, customer 9001
            new()
            {
                RentalId = 9001, RentalDate = DateTime.UtcNow.AddDays(-5), InventoryId = 9001,
                CustomerId = 9001, StaffId = 9001, ReturnDate = null, LastUpdate = DateTime.UtcNow
            },
            // Returned rental: Film A, store 1, inventory 9002, customer 9002
            new()
            {
                RentalId = 9002, RentalDate = DateTime.UtcNow.AddDays(-10), InventoryId = 9002,
                CustomerId = 9002, StaffId = 9001, ReturnDate = DateTime.UtcNow.AddDays(-3),
                LastUpdate = DateTime.UtcNow
            },
            // Active rental: Film B, store 1, inventory 9005, customer 9001
            new()
            {
                RentalId = 9003, RentalDate = DateTime.UtcNow.AddDays(-2), InventoryId = 9005,
                CustomerId = 9001, StaffId = 9001, ReturnDate = null, LastUpdate = DateTime.UtcNow
            },
        };
        db.Rentals.AddRange(rentals);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a rental with an associated payment record for delete-blocking tests.
    /// Call after SeedTestDataAsync.
    /// </summary>
    public static async Task SeedRentalWithPaymentAsync(DvdrentalContext db)
    {
        var rental = new Rental
        {
            RentalId = 9100, RentalDate = DateTime.UtcNow.AddDays(-20), InventoryId = 9003,
            CustomerId = 9001, StaffId = 9001, ReturnDate = DateTime.UtcNow.AddDays(-15),
            LastUpdate = DateTime.UtcNow
        };
        db.Rentals.Add(rental);
        await db.SaveChangesAsync();

        var payment = new Payment
        {
            PaymentId = 9001, CustomerId = 9001, StaffId = 9001,
            RentalId = 9100, Amount = 3.99m, PaymentDate = DateTime.UtcNow.AddDays(-15)
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds inventory with all copies rented out for a specific film+store combination.
    /// Film B at store 1 — both copies (9005, 9006) will have active rentals.
    /// Call after SeedTestDataAsync (which already rents 9005).
    /// </summary>
    public static async Task SeedAllCopiesRentedAsync(DvdrentalContext db)
    {
        // Inventory 9005 is already rented (rental 9003). Rent 9006 too.
        var rental = new Rental
        {
            RentalId = 9200, RentalDate = DateTime.UtcNow.AddDays(-1), InventoryId = 9006,
            CustomerId = 9002, StaffId = 9001, ReturnDate = null, LastUpdate = DateTime.UtcNow
        };
        db.Rentals.Add(rental);
        await db.SaveChangesAsync();
    }
}
