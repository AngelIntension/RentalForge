# Implementation Plan: Database Schema Creation & Seeding

**Branch**: `003-db-schema-seeding` | **Date**: 2026-02-22 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-db-schema-seeding/spec.md`

## Summary

Create an EF Core migration-based mechanism for initializing the dvdrental database schema with reference data (Country, City, Language, Category) pre-populated via `HasData()`. Provide an optional CLI command (`dotnet run -- --seed [--force]`) for populating all 15 tables with the full dvdrental dataset from embedded JSON files for development environments.

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (LTS, patch 10.0.3)
**Primary Dependencies**: EF Core 10.0, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0, System.Text.Json (built-in)
**Storage**: PostgreSQL 18 via EF Core with Npgsql provider
**Testing**: xUnit 2.9.3, FluentAssertions 8.8.0, Testcontainers.PostgreSql 4.10.0
**Target Platform**: Linux (WSL2)
**Project Type**: ASP.NET Core Web API (controller-based routing)
**Performance Goals**: Schema creation + reference data < 30s, full dev seed < 60s
**Constraints**: No runtime dependency on original dvdrental database; all seed data embedded in project
**Scale/Scope**: 731 reference data rows (4 tables), ~44K dev seed rows (11 tables)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-Driven Development | PASS | Spec completed and clarified before planning |
| II. Test-First (TDD) | PASS | Each phase starts with failing tests before implementation |
| III. Clean Architecture | PASS | Data seeding in Data layer, CLI parsing in presentation (Program.cs). No new layer boundaries needed. |
| IV. YAGNI | PASS | No new projects, no new NuGet packages, minimal abstractions. JSON files + static classes for data. |
| V. Observability | PASS | Structured logging via ILogger for seeding operations. Progress output to console. |
| VI. Functional Style | PASS | Reference data classes are pure static data. Seeder separates pure data loading from side-effecting DB writes. |
| Controller-based routing | N/A | No new API endpoints |
| Testcontainers | PASS | Tests use existing TestWebAppFactory with EnsureCreatedAsync(). HasData values applied automatically. |
| User secrets | PASS | No new secrets. Seeder uses existing connection string. |
| Dependency policy | PASS | No new NuGet packages. System.Text.Json is already available. |

**Post-Phase 1 re-check**: All gates still pass. No violations introduced by design decisions.

## Project Structure

### Documentation (this feature)

```text
specs/003-db-schema-seeding/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: research findings and decisions
├── data-model.md        # Phase 1: entity details and seed classification
├── quickstart.md        # Phase 1: developer quick reference
├── contracts/
│   └── cli-seed.md      # Phase 1: CLI command contract
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/RentalForge.Api/
├── Data/
│   ├── DvdrentalContext.cs              # MODIFIED: add HasData() calls
│   ├── Entities/                        # EXISTING: no changes
│   │   ├── Actor.cs
│   │   ├── Address.cs
│   │   ├── Category.cs
│   │   ├── City.cs
│   │   ├── Country.cs
│   │   ├── Customer.cs
│   │   ├── Film.cs
│   │   ├── FilmActor.cs
│   │   ├── FilmCategory.cs
│   │   ├── Inventory.cs
│   │   ├── Language.cs
│   │   ├── MpaaRating.cs
│   │   ├── Payment.cs
│   │   ├── Rental.cs
│   │   ├── Staff.cs
│   │   └── Store.cs
│   ├── ReferenceData/                   # NEW: static reference data classes
│   │   ├── CountryData.cs              #   109 country records
│   │   ├── CityData.cs                 #   600 city records
│   │   ├── LanguageData.cs             #   6 language records
│   │   └── CategoryData.cs            #   16 category records
│   ├── Seeding/                         # NEW: dev data seeder
│   │   ├── DevDataSeeder.cs            #   Seeding logic (seed/force-reset)
│   │   └── SeedData/                   #   Embedded JSON data files
│   │       ├── actors.json
│   │       ├── addresses.json
│   │       ├── films.json
│   │       ├── staff.json
│   │       ├── stores.json
│   │       ├── customers.json
│   │       ├── film_actors.json
│   │       ├── film_categories.json
│   │       ├── inventories.json
│   │       ├── rentals.json
│   │       └── payments.json
│   └── Migrations/                      # NEW: EF Core migrations
│       ├── YYYYMMDD_InitialCreate.cs
│       └── DvdrentalContextModelSnapshot.cs
├── Program.cs                           # MODIFIED: --seed [--force] CLI parsing
└── RentalForge.Api.csproj              # MODIFIED: EmbeddedResource for JSON files

tests/RentalForge.Api.Tests/
├── Infrastructure/
│   └── TestWebAppFactory.cs             # EXISTING: no changes needed
└── Integration/
    ├── DataLayerTests.cs                # EXISTING: no changes needed
    ├── HealthEndpointTests.cs           # EXISTING: no changes needed
    ├── ReferenceDataTests.cs            # NEW: tests for HasData reference data
    └── DevDataSeederTests.cs            # NEW: tests for CLI dev seeder
```

**Structure Decision**: Extends the existing single-project structure. New directories (`ReferenceData/`, `Seeding/`, `Migrations/`) are added under `Data/` to colocate data concerns. No new projects — YAGNI.

## Complexity Tracking

No constitution violations. No complexity justifications needed.

---

## Phase 1: Schema Migration + Reference Data (User Story 1)

**Goal**: Create the initial EF Core migration with all 15 tables and reference data for Country, City, Language, Category.

### 1.1 Create reference data source classes

Create static data classes in `Data/ReferenceData/` that return arrays of entity objects with hardcoded values from the original dvdrental database.

**Files**:
- `src/RentalForge.Api/Data/ReferenceData/LanguageData.cs` — 6 records
- `src/RentalForge.Api/Data/ReferenceData/CategoryData.cs` — 16 records
- `src/RentalForge.Api/Data/ReferenceData/CountryData.cs` — 109 records
- `src/RentalForge.Api/Data/ReferenceData/CityData.cs` — 600 records

Each class exposes a single `public static EntityType[] GetAll()` method returning the full dataset.

### 1.2 Add HasData() to OnModelCreating

Modify `DvdrentalContext.OnModelCreating()` to include `HasData()` calls referencing the source classes:

```csharp
modelBuilder.Entity<Language>().HasData(LanguageData.GetAll());
modelBuilder.Entity<Category>().HasData(CategoryData.GetAll());
modelBuilder.Entity<Country>().HasData(CountryData.GetAll());
modelBuilder.Entity<City>().HasData(CityData.GetAll());
```

### 1.3 Generate initial EF Core migration

```bash
dotnet ef migrations add InitialCreate --project src/RentalForge.Api --output-dir Data/Migrations
```

This captures the full schema (15 tables, relationships, constraints, custom types) plus `InsertData` calls for reference data.

**Verify**: Review the generated migration to confirm:
- All 15 tables created with correct columns and constraints
- `mpaa_rating` enum type created
- `InsertData` calls for Country (109), City (600), Language (6), Category (16)
- Sequence `setval()` calls to advance identity sequences past seeded IDs

### 1.4 Write integration tests (TDD: RED → GREEN)

**File**: `tests/RentalForge.Api.Tests/Integration/ReferenceDataTests.cs`

Tests use the existing `TestWebAppFactory` (which calls `EnsureCreatedAsync()`, automatically applying `HasData` values).

| Test | Verifies |
|------|----------|
| `ReferenceData_CountryTable_ContainsExpectedRowCount` | Country table has exactly 109 rows |
| `ReferenceData_CityTable_ContainsExpectedRowCount` | City table has exactly 600 rows |
| `ReferenceData_LanguageTable_ContainsExpectedRowCount` | Language table has exactly 6 rows |
| `ReferenceData_CategoryTable_ContainsExpectedRowCount` | Category table has exactly 16 rows |
| `ReferenceData_CitiesHaveValidCountryAssociations` | Every city's CountryId references an existing country |
| `ReferenceData_NonReferenceTables_AreEmpty` | All 11 non-reference tables have 0 rows |

**TDD flow**: Write tests first (RED — HasData not yet added). Add HasData in 1.2 (GREEN). Refactor as needed.

### 1.5 Verify existing tests still pass

Run `dotnet test` to confirm DataLayerTests and HealthEndpointTests are unaffected.

### Phase 1 Checkpoint

- [ ] All 15 tables created via migration
- [ ] 4 reference tables populated with correct data
- [ ] 11 non-reference tables empty
- [ ] All existing tests pass
- [ ] All new reference data tests pass

---

## Phase 2: Development Data Seeder (User Story 2)

**Goal**: Implement CLI-invokable seeding with `--seed [--force]` and embedded JSON data files.

### 2.1 Extract dev seed data from live dvdrental database

One-time data extraction using `psql` to export each non-reference table to JSON:

```bash
psql -h localhost -p 5432 -U postgres -d dvdrental \
  -c "COPY (SELECT json_agg(t) FROM actor t) TO STDOUT" > actors.json
```

Repeat for all 11 non-reference tables. Store JSON files in `src/RentalForge.Api/Data/Seeding/SeedData/`.

**Special handling**:
- Film.Fulltext (tsvector): Export as string representation. Handle deserialization with a custom converter or raw SQL insertion.
- Film.SpecialFeatures (text[]): JSON array format — deserializes naturally.
- Staff.Picture (byte[]): Export as Base64. May be null in original data.

### 2.2 Configure JSON files as embedded resources

Add to `RentalForge.Api.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Data\Seeding\SeedData\*.json" />
</ItemGroup>
```

### 2.3 Implement DevDataSeeder class

**File**: `src/RentalForge.Api/Data/Seeding/DevDataSeeder.cs`

Public interface:

```csharp
public class DevDataSeeder
{
    public DevDataSeeder(DvdrentalContext context, ILogger<DevDataSeeder> logger);

    /// <summary>
    /// Seeds dev data if tables are empty. Returns false if data already exists.
    /// </summary>
    public Task<bool> SeedAsync(CancellationToken ct = default);

    /// <summary>
    /// Truncates all non-reference tables and re-seeds from embedded data.
    /// </summary>
    public Task SeedForceAsync(CancellationToken ct = default);
}
```

**Implementation details**:
- Load JSON from embedded resources via `Assembly.GetManifestResourceStream()`
- Deserialize using `System.Text.Json.JsonSerializer`
- `SeedAsync`: Check `context.Actors.AnyAsync()` (or similar) to detect existing data. If data exists, log skip message and return false.
- `SeedForceAsync`: Execute `TRUNCATE actor, address, customer, film, film_actor, film_category, inventory, payment, rental, staff, store CASCADE` via raw SQL, then seed.
- Before bulk insert: `SET session_replication_role = 'replica'` (disable FK triggers)
- Insert tables in dependency order (tiers 2→7 per data-model.md)
- After bulk insert: `SET session_replication_role = 'DEFAULT'` (re-enable FK triggers)
- Use `AddRange` + `SaveChangesAsync` with reasonable batch sizes
- Log progress per table (table name, row count)
- Reset PostgreSQL identity sequences after seeding: `SELECT setval(pg_get_serial_sequence('{table}', '{pk_column}'), (SELECT COALESCE(MAX({pk_column}), 0) FROM {table}))`

### 2.4 Add CLI argument parsing to Program.cs

Modify `Program.cs` to check for `--seed` and `--force` in `args` after building the app:

```csharp
var app = builder.Build();

if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
    var force = args.Contains("--force");

    if (force)
        await seeder.SeedForceAsync();
    else
        await seeder.SeedAsync();

    return; // Exit without starting web server
}
```

Register `DevDataSeeder` in DI:
```csharp
builder.Services.AddScoped<DevDataSeeder>();
```

### 2.5 Write integration tests (TDD: RED → GREEN)

**File**: `tests/RentalForge.Api.Tests/Integration/DevDataSeederTests.cs`

Tests create a `DevDataSeeder` instance with a Testcontainers-backed `DvdrentalContext` and call seeder methods directly.

| Test | Verifies |
|------|----------|
| `Seed_PopulatesAllNonReferenceTables` | After `SeedAsync`, all 11 tables have expected row counts |
| `Seed_PreservesReferenceData` | After `SeedAsync`, reference tables still have correct data |
| `Seed_MaintainsForeignKeyIntegrity` | After `SeedAsync`, all FK relationships are valid |
| `Seed_SkipsWhenDataExists` | `SeedAsync` returns false when tables already populated |
| `Seed_SkipsWhenDataExists_LogsMessage` | Verify skip message is logged |
| `SeedForce_ClearsAndReseeds` | After `SeedForceAsync`, all tables have expected row counts |
| `SeedForce_PreservesReferenceData` | After `SeedForceAsync`, reference tables unchanged |
| `Seed_ReportsProgressPerTable` | Verify structured logging of per-table row counts |

### Phase 2 Checkpoint

- [ ] JSON data files present for all 11 non-reference tables
- [ ] DevDataSeeder.SeedAsync populates all tables correctly
- [ ] DevDataSeeder.SeedForceAsync clears and re-seeds correctly
- [ ] Skip-if-exists behavior works (returns false, logs message)
- [ ] CLI argument `--seed` triggers seeding and exits
- [ ] CLI argument `--seed --force` triggers force re-seed and exits
- [ ] All Phase 1 tests still pass
- [ ] All new seeder tests pass

---

## Phase 3: Idempotency and Edge Cases (User Story 3)

**Goal**: Verify all operations are safe to repeat and handle error conditions gracefully.

### 3.1 Write idempotency tests

**File**: Extend `tests/RentalForge.Api.Tests/Integration/ReferenceDataTests.cs` and `DevDataSeederTests.cs`

| Test | Verifies |
|------|----------|
| `ReferenceData_RunEnsureCreatedTwice_NoErrors` | Calling `EnsureCreatedAsync()` twice doesn't duplicate reference data |
| `Seed_RunTwiceWithoutForce_SecondCallSkips` | Second `SeedAsync` call returns false, data unchanged |
| `SeedForce_RunTwice_DataCorrectAfterEach` | Running `SeedForceAsync` twice leaves correct data both times |

### 3.2 Write edge case tests

| Test | Verifies |
|------|----------|
| `Seed_OnPartiallySeededDatabase_HandlesGracefully` | Seeder handles case where some tables have data and others don't |
| `SeedForce_OnPartiallySeededDatabase_CleansAndReseeds` | Force re-seed works correctly on partial data |

### 3.3 Error handling verification

Verify that DevDataSeeder provides clear error messages for:
- Missing schema (no tables exist)
- Interrupted seeding (transaction rollback)

### Phase 3 Checkpoint

- [ ] All idempotency tests pass
- [ ] Edge case tests pass
- [ ] Error messages are clear and actionable
- [ ] All previous tests still pass
- [ ] `dotnet test` passes with zero failures

---

## Final Verification

- [ ] `dotnet build` succeeds
- [ ] `dotnet test` — all tests pass
- [ ] `dotnet ef migrations list --project src/RentalForge.Api` shows InitialCreate
- [ ] SC-001: Schema creation + reference data < 30s (measured in test)
- [ ] SC-002: 4 reference tables populated, 11 empty (verified by tests)
- [ ] SC-003: Full seed < 60s (measured in test)
- [ ] SC-004: All 15 tables correct after seed (verified by tests)
- [ ] SC-005: Repeated operations produce no errors (verified by tests)
- [ ] SC-006: Progress output visible in console (manual verification)
