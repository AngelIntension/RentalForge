using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Data.Seeding;

public class DevDataSeeder
{
    private readonly DvdrentalContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DevDataSeeder> _logger;
    private static readonly Assembly Assembly = typeof(DevDataSeeder).Assembly;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public DevDataSeeder(DvdrentalContext context, UserManager<ApplicationUser> userManager, ILogger<DevDataSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> SeedAsync(CancellationToken ct = default)
    {
        if (await _context.Actors.AnyAsync(ct))
        {
            _logger.LogInformation("Database already contains development data. Skipping seed.");
            _logger.LogInformation("Use --seed --force to clear and re-seed.");
            return false;
        }

        _logger.LogInformation("Seeding development data...");
        await SeedAllTablesAsync(ct);
        return true;
    }

    public async Task SeedForceAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Force re-seeding development data...");
        _logger.LogInformation("Clearing existing non-reference data...");

        await _context.Database.ExecuteSqlRawAsync(
            "TRUNCATE actor, address, customer, film, film_actor, film_category, inventory, payment, rental, staff, store CASCADE",
            ct);

        // Detach all tracked entities after truncate
        _context.ChangeTracker.Clear();

        await SeedAllTablesAsync(ct);
    }

    private async Task SeedAllTablesAsync(CancellationToken ct)
    {
        var totalRows = 0;

        // Keep the connection open so SET session_replication_role applies to all subsequent operations
        await _context.Database.OpenConnectionAsync(ct);

        try
        {
            await _context.Database.ExecuteSqlRawAsync("SET session_replication_role = 'replica'", ct);
            totalRows += await SeedTableAsync<Actor, ActorDto>(
                "actors.json", "Actor", dto => new Actor
                {
                    ActorId = dto.ActorId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    LastUpdate = DateTime.SpecifyKind(dto.LastUpdate, DateTimeKind.Utc),
                }, ct);

            totalRows += await SeedTableAsync<Address, AddressDto>(
                "addresses.json", "Address", dto => new Address
                {
                    AddressId = dto.AddressId,
                    Address1 = dto.Address,
                    Address2 = dto.Address2,
                    District = dto.District,
                    CityId = dto.CityId,
                    PostalCode = dto.PostalCode,
                    Phone = dto.Phone,
                    LastUpdate = DateTime.SpecifyKind(dto.LastUpdate, DateTimeKind.Utc),
                }, ct);

            totalRows += await SeedTableAsync<Film, FilmDto>(
                "films.json", "Film", dto => new Film
                {
                    FilmId = dto.FilmId,
                    Title = dto.Title,
                    Description = dto.Description,
                    ReleaseYear = dto.ReleaseYear,
                    LanguageId = dto.LanguageId,
                    OriginalLanguageId = null,
                    RentalDuration = dto.RentalDuration,
                    RentalRate = dto.RentalRate,
                    Length = dto.Length,
                    ReplacementCost = dto.ReplacementCost,
                    Rating = ParseRating(dto.Rating),
                    LastUpdate = DateTime.SpecifyKind(dto.LastUpdate, DateTimeKind.Utc),
                    SpecialFeatures = dto.SpecialFeatures,
#pragma warning disable CS0618 // NpgsqlTsVector.Parse is fine for importing pre-computed tsvector strings
                    Fulltext = NpgsqlTsVector.Parse(dto.Fulltext),
#pragma warning restore CS0618
                }, ct);

            totalRows += await SeedTableAsync<Staff, StaffDto>(
                "staff.json", "Staff", dto => new Staff
                {
                    StaffId = dto.StaffId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    AddressId = dto.AddressId,
                    Email = dto.Email,
                    StoreId = dto.StoreId,
                    Active = dto.Active,
                    Username = dto.Username,
                    Password = dto.Password,
                    LastUpdate = DateTime.SpecifyKind(dto.LastUpdate, DateTimeKind.Utc),
                    Picture = dto.Picture != null ? Convert.FromBase64String(dto.Picture) : null,
                }, ct);

            totalRows += await SeedTableAsync<Store, StoreDto>(
                "stores.json", "Store", dto => new Store
                {
                    StoreId = dto.StoreId,
                    ManagerStaffId = dto.ManagerStaffId,
                    AddressId = dto.AddressId,
                    LastUpdate = DateTime.SpecifyKind(dto.LastUpdate, DateTimeKind.Utc),
                }, ct);

            totalRows += await SeedTableAsync<Customer, CustomerDto>(
                "customers.json", "Customer", dto => new Customer
                {
                    CustomerId = dto.CustomerId,
                    StoreId = dto.StoreId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    AddressId = dto.AddressId,
                    Activebool = dto.Activebool,
                    CreateDate = DateOnly.Parse(dto.CreateDate),
                    LastUpdate = DateTime.SpecifyKind(dto.LastUpdate, DateTimeKind.Utc),
                    Active = dto.Active,
                }, ct);

            totalRows += await SeedTableAsync<FilmActor, FilmActorDto>(
                "film_actors.json", "FilmActor", dto => new FilmActor
                {
                    ActorId = dto.ActorId,
                    FilmId = dto.FilmId,
                    LastUpdate = DateTime.SpecifyKind(dto.LastUpdate, DateTimeKind.Utc),
                }, ct);

            totalRows += await SeedTableAsync<FilmCategory, FilmCategoryDto>(
                "film_categories.json", "FilmCategory", dto => new FilmCategory
                {
                    FilmId = dto.FilmId,
                    CategoryId = dto.CategoryId,
                    LastUpdate = DateTime.SpecifyKind(dto.LastUpdate, DateTimeKind.Utc),
                }, ct);

            totalRows += await SeedTableAsync<Inventory, InventoryDto>(
                "inventories.json", "Inventory", dto => new Inventory
                {
                    InventoryId = dto.InventoryId,
                    FilmId = dto.FilmId,
                    StoreId = dto.StoreId,
                    LastUpdate = DateTime.SpecifyKind(dto.LastUpdate, DateTimeKind.Utc),
                }, ct);

            totalRows += await SeedTableAsync<Rental, RentalDto>(
                "rentals.json", "Rental", dto => new Rental
                {
                    RentalId = dto.RentalId,
                    RentalDate = DateTime.SpecifyKind(dto.RentalDate, DateTimeKind.Utc),
                    InventoryId = dto.InventoryId,
                    CustomerId = dto.CustomerId,
                    ReturnDate = dto.ReturnDate.HasValue
                        ? DateTime.SpecifyKind(dto.ReturnDate.Value, DateTimeKind.Utc)
                        : null,
                    StaffId = dto.StaffId,
                    LastUpdate = DateTime.SpecifyKind(dto.LastUpdate, DateTimeKind.Utc),
                }, ct);

            totalRows += await SeedTableAsync<Payment, PaymentDto>(
                "payments.json", "Payment", dto => new Payment
                {
                    PaymentId = dto.PaymentId,
                    CustomerId = dto.CustomerId,
                    StaffId = dto.StaffId,
                    RentalId = dto.RentalId,
                    Amount = dto.Amount,
                    PaymentDate = DateTime.SpecifyKind(dto.PaymentDate, DateTimeKind.Utc),
                }, ct);
        }
        finally
        {
            await _context.Database.ExecuteSqlRawAsync("SET session_replication_role = 'origin'", ct);
            await _context.Database.CloseConnectionAsync();
        }

        // Reset identity sequences
        await ResetSequencesAsync(ct);

        // Seed auth users after all dvdrental data (customer link needs existing customer)
        await SeedAuthUsersAsync();

        _logger.LogInformation("Development data seeded successfully. Total: {TotalRows:N0} rows.", totalRows);
    }

    private async Task SeedAuthUsersAsync()
    {
        var defaultPassword = "DevP@ss1";
        var users = new (string Email, string Role, int? CustomerId, int? StaffId)[]
        {
            ("admin@rentalforge.dev", "Admin", null, 1),
            ("staff@rentalforge.dev", "Staff", null, 1),
            ("customer@rentalforge.dev", "Customer", 1, null),
        };

        foreach (var (email, role, customerId, staffId) in users)
        {
            if (await _userManager.FindByEmailAsync(email) is not null)
            {
                _logger.LogInformation("  Auth user {Email} already exists, skipping", email);
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                CustomerId = customerId,
                StaffId = staffId,
                CreatedAt = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(user, defaultPassword);
            if (!result.Succeeded)
            {
                _logger.LogWarning("  Failed to create auth user {Email}: {Errors}",
                    email, string.Join(", ", result.Errors.Select(e => e.Description)));
                continue;
            }

            await _userManager.AddToRoleAsync(user, role);
            _logger.LogInformation("  Auth user {Email} created with role {Role}", email, role);
        }
    }

    private async Task<int> SeedTableAsync<TEntity, TDto>(
        string fileName, string tableName, Func<TDto, TEntity> map, CancellationToken ct)
        where TEntity : class
    {
        var resourceName = $"RentalForge.Api.Data.Seeding.SeedData.{fileName}";
        await using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Seed failed: unable to load seed data for {tableName}. Resource '{resourceName}' not found.");

        var dtos = await JsonSerializer.DeserializeAsync<List<TDto>>(stream, JsonOptions, ct)
            ?? throw new InvalidOperationException($"Seed failed: unable to load seed data for {tableName}. JSON deserialization returned null.");

        var entities = dtos.Select(map).ToList();
        _context.Set<TEntity>().AddRange(entities);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("  {TableName}: {Count:N0} rows inserted", tableName, entities.Count);
        return entities.Count;
    }

    private async Task ResetSequencesAsync(CancellationToken ct)
    {
        var sequences = new (string table, string column)[]
        {
            ("actor", "actor_id"),
            ("address", "address_id"),
            ("film", "film_id"),
            ("staff", "staff_id"),
            ("store", "store_id"),
            ("customer", "customer_id"),
            ("inventory", "inventory_id"),
            ("rental", "rental_id"),
            ("payment", "payment_id"),
        };

        foreach (var (table, column) in sequences)
        {
#pragma warning disable EF1002 // Table/column names are hardcoded constants, not user input
            await _context.Database.ExecuteSqlRawAsync(
                $"SELECT setval(pg_get_serial_sequence('{table}', '{column}'), (SELECT COALESCE(MAX({column}), 0) FROM {table}))",
                ct);
#pragma warning restore EF1002
        }
    }

    private static MpaaRating? ParseRating(string? rating) => rating switch
    {
        "G" => MpaaRating.G,
        "PG" => MpaaRating.PG,
        "PG-13" => MpaaRating.Pg13,
        "R" => MpaaRating.R,
        "NC-17" => MpaaRating.Nc17,
        _ => null,
    };

    // DTOs for JSON deserialization (snake_case field names from PostgreSQL)
    private sealed record ActorDto(int ActorId, string FirstName, string LastName, DateTime LastUpdate);
    private sealed record AddressDto(int AddressId, string Address, string? Address2, string District, int CityId, string? PostalCode, string Phone, DateTime LastUpdate);
    private sealed record FilmDto(int FilmId, string Title, string? Description, int? ReleaseYear, int LanguageId, short RentalDuration, decimal RentalRate, short? Length, decimal ReplacementCost, string? Rating, DateTime LastUpdate, string[]? SpecialFeatures, string Fulltext);
    private sealed record StaffDto(int StaffId, string FirstName, string LastName, int AddressId, string? Email, int StoreId, bool Active, string Username, string? Password, DateTime LastUpdate, string? Picture);
    private sealed record StoreDto(int StoreId, int ManagerStaffId, int AddressId, DateTime LastUpdate);
    private sealed record CustomerDto(int CustomerId, int StoreId, string FirstName, string LastName, string? Email, int AddressId, bool Activebool, string CreateDate, DateTime LastUpdate, int? Active);
    private sealed record FilmActorDto(int ActorId, int FilmId, DateTime LastUpdate);
    private sealed record FilmCategoryDto(int FilmId, int CategoryId, DateTime LastUpdate);
    private sealed record InventoryDto(int InventoryId, int FilmId, int StoreId, DateTime LastUpdate);
    private sealed record RentalDto(int RentalId, DateTime RentalDate, int InventoryId, int CustomerId, DateTime? ReturnDate, int StaffId, DateTime LastUpdate);
    private sealed record PaymentDto(int PaymentId, int CustomerId, int StaffId, int RentalId, decimal Amount, DateTime PaymentDate);
}
