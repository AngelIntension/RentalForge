# Research: Database Schema Creation & Seeding

**Feature**: 003-db-schema-seeding | **Date**: 2026-02-22

## Decision 1: Reference Data Mechanism

**Decision**: Use `HasData()` in `OnModelCreating` (EF Core model managed data)

**Rationale**:
- Works with both `EnsureCreatedAsync()` (current test infrastructure) AND `dotnet ef database update` (production migrations)
- Idempotent by design — migrations track applied state via `__EFMigrationsHistory`, and `EnsureCreatedAsync` applies HasData values automatically
- No additional packages required
- Reference data (731 rows across 4 tables) is genuinely static — exactly the use case HasData was designed for

**Alternatives considered**:
- **Custom migration with raw SQL INSERT**: Full PostgreSQL control (`ON CONFLICT DO NOTHING`), but does NOT work with `EnsureCreatedAsync()` — would require changing test infrastructure to `MigrateAsync()`, which is a larger architectural change
- **`MigrationBuilder.InsertData()` in migration code**: Same `EnsureCreatedAsync()` incompatibility as raw SQL, plus more verbose than raw SQL with no added benefit

**Known limitations**:
- Requires explicit primary key values (IDs from original dvdrental database — available)
- PostgreSQL sequence collision risk (npgsql/efcore.pg#759): Npgsql generates `SELECT setval()` in migrations to advance sequences past seeded values. For static reference data that never gets new rows via the app, this is not a concern.
- Model snapshot grows with seeded data (~731 rows). Acceptable for 4 reference tables.

## Decision 2: CLI Dev Seeding Mechanism

**Decision**: Custom CLI argument `dotnet run -- --seed [--force]` parsed in `Program.cs`

**Rationale**:
- Simplest approach satisfying all requirements (YAGNI)
- No new projects, no new NuGet packages
- Full access to existing DI container (DbContext, logging, configuration)
- Runs between `builder.Build()` and `app.Run()` — all services available, no HTTP requests served during seeding
- Seeder logic extractable into a reusable class that tests can also call
- Natural force-reset semantics: `--seed` for skip-if-exists, `--seed --force` for truncate-and-reseed

**Alternatives considered**:
- **`UseSeeding`/`UseAsyncSeeding` (EF Core 9+ built-in)**: Runs on every `Migrate()` and `EnsureCreated()` call — no opt-in mechanism. Would run in production during migrations. No mechanism to pass a force flag. Conflates always-on reference data with opt-in dev data.
- **Separate console project**: Violates YAGNI. Requires duplicating DbContext configuration, connection string setup, and enum mapping. Poor Testcontainers integration.
- **`IHostedService`-based seeder**: Timing issue — runs after web server starts accepting requests. Config-flag approach less ergonomic than CLI argument for one-time operations.

## Decision 3: Dev Seed Data Format

**Decision**: JSON files embedded in the project (one file per table, 11 non-reference tables)

**Rationale**:
- Human-readable, easy to version control and diff
- Deserializable directly into EF Core entities via System.Text.Json (already available)
- EF Core `AddRange` + `SaveChangesAsync` for insertion (idiomatic, works with change tracking and batching)
- Total estimated size: ~1-2 MB of JSON for ~44K rows — manageable for git

**Alternatives considered**:
- **Raw SQL INSERT files**: More efficient for bulk loading, but bypasses EF Core change tracking. PostgreSQL-specific syntax.
- **C# static arrays**: Type-safe but extremely verbose for ~44K rows. Poor readability and maintainability.
- **CSV files**: Compact but no native type information, requires manual parsing for complex types (arrays, enums, dates).

## Decision 4: Circular Dependency Handling (Staff ↔ Store)

**Decision**: Use PostgreSQL `SET session_replication_role = 'replica'` during bulk dev seeding to disable FK triggers

**Rationale**:
- Staff.StoreId (required) → Store and Store.ManagerStaffId (required) → Staff create a circular FK dependency
- Both columns are non-nullable, preventing a two-pass insert approach
- `session_replication_role = 'replica'` is PostgreSQL's standard mechanism for bulk loading — disables all FK triggers for the session
- After bulk insert, re-enable with `SET session_replication_role = 'DEFAULT'`
- Only needed for dev seeding (reference data has no circular dependencies)

## Decision 5: Reference Data Organization

**Decision**: Separate static data classes in `Data/ReferenceData/` directory, one per entity

**Rationale**:
- OnModelCreating stays readable — calls `HasData(CountryData.GetAll())` instead of inlining 731 data entries
- Each file is focused: `CountryData.cs` (109 entries), `CityData.cs` (600 entries), `LanguageData.cs` (6 entries), `CategoryData.cs` (16 entries)
- Standard EF Core practice for organizing large HasData datasets
- Not over-engineering — it's just data organization, no abstractions or interfaces

## Database Row Counts (Source of Truth)

| Table         | Rows   | Classification |
|---------------|--------|----------------|
| Country       | 109    | Reference      |
| City          | 600    | Reference      |
| Language      | 6      | Reference      |
| Category      | 16     | Reference      |
| Actor         | 200    | Dev seed       |
| Address       | 603    | Dev seed       |
| Customer      | 599    | Dev seed       |
| Film          | 1,000  | Dev seed       |
| FilmActor     | 5,462  | Dev seed       |
| FilmCategory  | 1,000  | Dev seed       |
| Inventory     | 4,581  | Dev seed       |
| Rental        | 16,044 | Dev seed       |
| Payment       | 14,596 | Dev seed       |
| Staff         | 2      | Dev seed       |
| Store         | 2      | Dev seed       |
| **Total**     | **44,820** | |

## Seeding Dependency Order

Based on FK analysis, tables must be seeded in this order:

**Tier 0 — No FK dependencies (reference, via HasData):** Country, Language, Category, Actor*
**Tier 1 — Depends on Tier 0 (reference, via HasData):** City → Country
**Tier 2 — Depends on reference data:** Address → City, Film → Language
**Tier 3 — Depends on Tier 2:** Staff → Address (circular with Store)
**Tier 4 — Depends on Tier 3:** Store → Address + Staff, Customer → Store + Address
**Tier 5 — Depends on Tier 2-4:** FilmActor → Actor + Film, FilmCategory → Film + Category, Inventory → Film + Store
**Tier 6 — Depends on Tier 4-5:** Rental → Inventory + Customer + Staff
**Tier 7 — Depends on Tier 6:** Payment → Customer + Rental + Staff

*Actor has no FK dependencies but is not reference data — it's part of dev seeding.

**Note**: The Staff ↔ Store circular dependency requires disabling FK constraints during dev seeding (see Decision 4).

## Custom PostgreSQL Types

| Type          | Kind   | Handled By                    |
|---------------|--------|-------------------------------|
| `mpaa_rating` | Enum   | Mapped in NpgsqlDataSourceBuilder + EF model |
| `year`        | Domain | Not in EF model (mapped as int?). Migration creates integer column — functionally equivalent. |

## Test Infrastructure Impact

- **No changes to TestWebAppFactory**: `EnsureCreatedAsync()` continues to work. HasData values are automatically applied.
- **Dev seeder testability**: Extract seeding logic into `DevDataSeeder` class with `SeedAsync(DvdrentalContext, bool force)` method. Tests can call this directly.
- **JSON seed data in tests**: Tests that need full dev data load the same JSON files used by the CLI seeder.

## Sources

- [Data Seeding - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)
- [npgsql/efcore.pg#759 — HasData sequence collision issue](https://github.com/npgsql/efcore.pg/issues/759)
- [New Data Seeding Methods in EF Core 9 — Felipe Gavilan](https://gavilan.blog/2024/11/22/new-data-seeding-methods-in-entity-framework-core-9/)
