using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Tests.Infrastructure;

/// <summary>
/// Seeds test data for customer integration tests.
/// Handles Store↔Staff circular FK with session_replication_role.
/// </summary>
public static class CustomerTestHelper
{
    /// <summary>
    /// Seeds a minimal set of related entities (Address, Store, Staff, Customers)
    /// required for customer CRUD integration tests.
    /// </summary>
    public static async Task SeedTestDataAsync(DvdrentalContext db, int customerCount = 3)
    {
        // Use CityId=1 from reference data (seeded by HasData)
        var storeAddress = new Address
        {
            AddressId = 9001,
            Address1 = "100 Test Store St",
            District = "TestDistrict",
            CityId = 1,
            Phone = "555-0100",
            LastUpdate = DateTime.UtcNow
        };

        var staffAddress = new Address
        {
            AddressId = 9002,
            Address1 = "200 Test Staff Ave",
            District = "TestDistrict",
            CityId = 1,
            Phone = "555-0200",
            LastUpdate = DateTime.UtcNow
        };

        var customerAddresses = Enumerable.Range(0, customerCount)
            .Select(i => new Address
            {
                AddressId = 9100 + i,
                Address1 = $"{300 + i} Customer Lane",
                District = "TestDistrict",
                CityId = 1,
                Phone = $"555-{9100 + i}",
                LastUpdate = DateTime.UtcNow
            })
            .ToList();

        db.Addresses.AddRange([storeAddress, staffAddress, .. customerAddresses]);
        await db.SaveChangesAsync();

        // Insert Store and Staff via raw SQL in a single batch to avoid
        // EF Core's topological sort cycle and ensure session_replication_role
        // applies on the same connection
        await db.Database.ExecuteSqlRawAsync(
            """
            SET session_replication_role = 'replica';
            INSERT INTO store (store_id, manager_staff_id, address_id, last_update)
            VALUES (9001, 9001, 9001, NOW());
            INSERT INTO staff (staff_id, first_name, last_name, address_id, store_id, active, username, last_update)
            VALUES (9001, 'Test', 'Manager', 9002, 9001, true, 'testmgr', NOW());
            SET session_replication_role = 'origin';
            """);

        // Seed customers
        var customers = new List<Customer>
        {
            new()
            {
                CustomerId = 9001,
                StoreId = 9001,
                FirstName = "Alice",
                LastName = "Anderson",
                Email = "alice@example.com",
                AddressId = 9100,
                Activebool = true,
                Active = 1,
                CreateDate = new DateOnly(2024, 1, 1),
                LastUpdate = DateTime.UtcNow
            },
            new()
            {
                CustomerId = 9002,
                StoreId = 9001,
                FirstName = "Bob",
                LastName = "Brown",
                Email = "bob@example.com",
                AddressId = 9101,
                Activebool = true,
                Active = 1,
                CreateDate = new DateOnly(2024, 1, 2),
                LastUpdate = DateTime.UtcNow
            },
            new()
            {
                CustomerId = 9003,
                StoreId = 9001,
                FirstName = "Charlie",
                LastName = "Clark",
                Email = "charlie@example.com",
                AddressId = 9102,
                Activebool = false,
                Active = 0,
                CreateDate = new DateOnly(2024, 1, 3),
                LastUpdate = DateTime.UtcNow
            }
        };

        db.Customers.AddRange(customers.Take(customerCount));
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds many customers for pagination testing.
    /// </summary>
    public static async Task SeedManyCustomersAsync(DvdrentalContext db, int count)
    {
        // Add extra addresses and customers beyond the initial 3
        var extraAddresses = Enumerable.Range(3, count - 3)
            .Select(i => new Address
            {
                AddressId = 9100 + i,
                Address1 = $"{300 + i} Customer Lane",
                District = "TestDistrict",
                CityId = 1,
                Phone = $"555-{9100 + i}",
                LastUpdate = DateTime.UtcNow
            })
            .ToList();

        db.Addresses.AddRange(extraAddresses);
        await db.SaveChangesAsync();

        var extraCustomers = Enumerable.Range(3, count - 3)
            .Select(i => new Customer
            {
                CustomerId = 9001 + i,
                StoreId = 9001,
                FirstName = $"Customer{i:D3}",
                LastName = $"Test{i:D3}",
                Email = $"customer{i}@example.com",
                AddressId = 9100 + i,
                Activebool = true,
                Active = 1,
                CreateDate = new DateOnly(2024, 1, 1),
                LastUpdate = DateTime.UtcNow
            })
            .ToList();

        db.Customers.AddRange(extraCustomers);
        await db.SaveChangesAsync();
    }
}
