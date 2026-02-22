# Quick Reference: 003-db-schema-seeding

## What This Feature Does

1. **Initial migration**: Creates the dvdrental schema via EF Core migration with reference data (Country, City, Language, Category) pre-populated
2. **Dev seeder**: CLI command to optionally populate all 15 tables with the full dvdrental dataset

## Key Commands

```bash
# Apply schema migration (creates tables + reference data)
dotnet ef database update --project src/RentalForge.Api

# Seed development data (skip if already exists)
dotnet run --project src/RentalForge.Api -- --seed

# Force re-seed (clear non-reference data and re-insert)
dotnet run --project src/RentalForge.Api -- --seed --force

# Run tests
dotnet test
```

## Key Files

| File | Purpose |
|------|---------|
| `src/RentalForge.Api/Data/DvdrentalContext.cs` | Modified: HasData() calls for reference data |
| `src/RentalForge.Api/Data/ReferenceData/*.cs` | Reference data values (Country, City, Language, Category) |
| `src/RentalForge.Api/Data/Seeding/DevDataSeeder.cs` | Dev seeding logic (seed/force-reset) |
| `src/RentalForge.Api/Data/Seeding/SeedData/*.json` | Embedded JSON data for 11 non-reference tables |
| `src/RentalForge.Api/Data/Migrations/*` | EF Core migration files |
| `src/RentalForge.Api/Program.cs` | Modified: --seed CLI argument parsing |

## Architecture Decisions

- **Reference data**: `HasData()` in OnModelCreating → works with both migrations and `EnsureCreatedAsync()` (tests)
- **Dev seeding**: `dotnet run -- --seed [--force]` → parsed in Program.cs, runs DevDataSeeder before web server starts
- **Data format**: JSON files for dev seed data, C# static classes for reference data
- **Circular dependency** (Staff ↔ Store): Handled via `SET session_replication_role = 'replica'` during dev seeding
- **Test compatibility**: No changes to TestWebAppFactory — `EnsureCreatedAsync()` automatically applies HasData reference data
