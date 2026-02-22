# CLI Contract: Database Seeding Command

**Feature**: 003-db-schema-seeding | **Date**: 2026-02-22

## Command Signatures

### Seed Development Data (skip-if-exists mode)

```bash
dotnet run --project src/RentalForge.Api -- --seed
```

**Behavior**: Checks if non-reference tables contain data. If data exists, skips seeding and reports status. If tables are empty, inserts full dvdrental dataset from embedded JSON files.

**Exit**: After seeding completes (or skips), the process exits without starting the web server.

**Output (data exists)**:
```
Database already contains development data. Skipping seed.
Use --seed --force to clear and re-seed.
```

**Output (seeding)**:
```
Seeding development data...
  Actor:        200 rows inserted
  Address:      603 rows inserted
  Film:         1,000 rows inserted
  Staff:        2 rows inserted
  Store:        2 rows inserted
  Customer:     599 rows inserted
  FilmActor:    5,462 rows inserted
  FilmCategory: 1,000 rows inserted
  Inventory:    4,581 rows inserted
  Rental:       16,044 rows inserted
  Payment:      14,596 rows inserted
Development data seeded successfully. Total: 44,089 rows.
```

### Force Re-seed (clear and re-seed mode)

```bash
dotnet run --project src/RentalForge.Api -- --seed --force
```

**Behavior**: Truncates all non-reference tables (preserving Country, City, Language, Category), then inserts the full dvdrental dataset from embedded JSON files.

**Output**:
```
Force re-seeding development data...
Clearing existing non-reference data...
  Actor:        200 rows inserted
  Address:      603 rows inserted
  ...
Development data seeded successfully. Total: 44,089 rows.
```

### Normal Application Start (no seeding)

```bash
dotnet run --project src/RentalForge.Api
```

**Behavior**: Unchanged — starts the web server normally. No seeding occurs.

## Error Conditions

| Condition | Exit Code | Message |
|-----------|-----------|---------|
| Connection string missing/empty | Non-zero | Existing validation error (unchanged) |
| Database unreachable | Non-zero | `Seed failed: unable to connect to database. {details}` |
| Schema not applied (no tables) | Non-zero | `Seed failed: database schema not found. Run 'dotnet ef database update' first.` |
| Seed data files missing/corrupt | Non-zero | `Seed failed: unable to load seed data for {table}. {details}` |
| Partial failure during seeding | Non-zero | `Seed failed during {table}: {details}. Database may be in an inconsistent state. Use --seed --force to reset.` |

## Argument Parsing Rules

- `--seed` must appear in application arguments (after `--` separator in `dotnet run`)
- `--force` only has effect when `--seed` is also present
- `--force` without `--seed` is ignored
- When `--seed` is present, the application exits after seeding (does not start web server)
- All other arguments are passed through to the ASP.NET Core host as normal
