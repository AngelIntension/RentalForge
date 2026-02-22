# Tasks: Database Schema Creation & Seeding

**Input**: Design documents from `/specs/003-db-schema-seeding/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/cli-seed.md

**Tests**: Included — constitution mandates TDD (Test-First is NON-NEGOTIABLE).

**Organization**: Tasks grouped by user story. US2 depends on US1 (schema + reference data required). US3 depends on US1 + US2 (validates idempotency of both).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directory structure for new files

- [X] T001 Create directory structure: `src/RentalForge.Api/Data/ReferenceData/`, `src/RentalForge.Api/Data/Seeding/SeedData/`

---

## Phase 2: User Story 1 — Create Empty Database with Reference Data (Priority: P1) 🎯 MVP

**Goal**: Create the initial EF Core migration with all 15 tables and reference data (Country 109, City 600, Language 6, Category 16) pre-populated via `HasData()`.

**Independent Test**: Trigger database creation against an empty Testcontainers instance and verify all 15 tables exist, 4 reference tables contain expected row counts, and 11 non-reference tables are empty.

### Tests for User Story 1 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T002 [US1] Write integration tests for reference data verification in `tests/RentalForge.Api.Tests/Integration/ReferenceDataTests.cs` — tests: CountryTable has 109 rows, CityTable has 600 rows, LanguageTable has 6 rows, CategoryTable has 16 rows, cities have valid country associations, all 11 non-reference tables are empty. Use existing `TestWebAppFactory` (IClassFixture). Tests should RED because HasData not yet added.

### Implementation for User Story 1

- [X] T003 [P] [US1] Create `LanguageData.cs` with static `GetAll()` returning 6 Language records (English, Italian, Japanese, Mandarin, French, German with original IDs and LastUpdate timestamps) in `src/RentalForge.Api/Data/ReferenceData/LanguageData.cs`
- [X] T004 [P] [US1] Create `CategoryData.cs` with static `GetAll()` returning 16 Category records (Action through Travel with original IDs and LastUpdate timestamps) in `src/RentalForge.Api/Data/ReferenceData/CategoryData.cs`
- [X] T005 [P] [US1] Create `CountryData.cs` with static `GetAll()` returning 109 Country records (all countries from original dvdrental with original IDs and LastUpdate timestamps) in `src/RentalForge.Api/Data/ReferenceData/CountryData.cs`
- [X] T006 [P] [US1] Create `CityData.cs` with static `GetAll()` returning 600 City records (all cities with correct CountryId associations, original IDs and LastUpdate timestamps) in `src/RentalForge.Api/Data/ReferenceData/CityData.cs`
- [X] T007 [US1] Add `HasData()` calls to `OnModelCreating` referencing the 4 reference data classes in `src/RentalForge.Api/Data/DvdrentalContext.cs` — add: `modelBuilder.Entity<Language>().HasData(LanguageData.GetAll())`, same for Category, Country, City
- [X] T008 [US1] Generate initial EF Core migration via `dotnet ef migrations add InitialCreate --project src/RentalForge.Api --output-dir Data/Migrations` — verify generated migration in `src/RentalForge.Api/Data/Migrations/` contains all 15 tables, mpaa_rating enum, InsertData calls for 731 reference rows, and sequence setval calls
- [X] T009 [US1] Run all tests — verify reference data tests pass (GREEN) and all existing tests (DataLayerTests, HealthEndpointTests) remain unaffected

**Checkpoint**: Schema creation + reference data working. 4 reference tables populated, 11 non-reference tables empty. All tests pass.

---

## Phase 3: User Story 2 — Seed Full Development Data via Command Line (Priority: P2)

**Goal**: Implement CLI-invokable seeding with `dotnet run -- --seed [--force]` using embedded JSON data files for all 11 non-reference tables (~44K rows).

**Independent Test**: Create database with reference data (US1), invoke DevDataSeeder, verify all 15 tables contain expected row counts matching original dvdrental database.

### Data Preparation for User Story 2

- [X] T010 [P] [US2] Extract actor (200), address (603), and film (1000) data from live dvdrental DB to JSON files: `src/RentalForge.Api/Data/Seeding/SeedData/actors.json`, `addresses.json`, `films.json` — use `psql` to export. Handle Film.SpecialFeatures (text[]) as JSON arrays and Film.Fulltext (tsvector) as string representation.
- [X] T011 [P] [US2] Extract staff (2), store (2), and customer (599) data from live dvdrental DB to JSON files: `src/RentalForge.Api/Data/Seeding/SeedData/staff.json`, `stores.json`, `customers.json`
- [X] T012 [P] [US2] Extract film_actor (5462), film_category (1000), and inventory (4581) data from live dvdrental DB to JSON files: `src/RentalForge.Api/Data/Seeding/SeedData/film_actors.json`, `film_categories.json`, `inventories.json`
- [X] T013 [P] [US2] Extract rental (16044) and payment (14596) data from live dvdrental DB to JSON files: `src/RentalForge.Api/Data/Seeding/SeedData/rentals.json`, `payments.json`
- [X] T014 [US2] Configure all JSON files as embedded resources by adding `<EmbeddedResource Include="Data\Seeding\SeedData\*.json" />` to `src/RentalForge.Api/RentalForge.Api.csproj`

### Tests for User Story 2 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T015 [US2] Write integration tests for DevDataSeeder in `tests/RentalForge.Api.Tests/Integration/DevDataSeederTests.cs` — tests: SeedAsync populates all 11 non-reference tables with expected row counts, SeedAsync preserves reference data, SeedAsync maintains FK integrity, SeedAsync returns false and skips when data already exists, SeedForceAsync clears non-reference data and re-seeds correctly, SeedForceAsync preserves reference data, seeder logs progress per table. Use existing `TestWebAppFactory` for Testcontainers-backed DbContext. Tests should RED because DevDataSeeder not yet implemented.

### Implementation for User Story 2

- [X] T016 [US2] Implement `DevDataSeeder` class in `src/RentalForge.Api/Data/Seeding/DevDataSeeder.cs` — constructor takes `DvdrentalContext` and `ILogger<DevDataSeeder>`. `SeedAsync`: loads JSON from embedded resources via `Assembly.GetManifestResourceStream()`, checks if data exists via `AnyAsync()`, inserts in dependency order (Actor/Address/Film → Staff/Store/Customer → FilmActor/FilmCategory/Inventory → Rental → Payment) using `AddRange` + `SaveChangesAsync`. Disable FK triggers via `SET session_replication_role = 'replica'` before bulk insert, re-enable after. Reset identity sequences via `SELECT setval()`. Log per-table progress. `SeedForceAsync`: executes `TRUNCATE ... CASCADE` on all 11 non-reference tables, then calls seed logic. Per contracts/cli-seed.md output format.
- [X] T017 [US2] Register `DevDataSeeder` as scoped service and add `--seed [--force]` CLI argument parsing in `src/RentalForge.Api/Program.cs` — after `builder.Build()`, check `args.Contains("--seed")`, resolve DevDataSeeder from DI scope, call SeedAsync or SeedForceAsync based on `--force` flag, exit without starting web server (per contracts/cli-seed.md). Structured logging for all operations.
- [X] T018 [US2] Run all tests — verify dev seeder tests pass (GREEN), reference data tests still pass, and all existing tests remain unaffected

**Checkpoint**: Full dev seeding working. `dotnet run -- --seed` populates all tables. `--seed --force` clears and re-seeds. Skip-if-exists behavior works. All tests pass.

---

## Phase 4: User Story 3 — Idempotent and Safe Operations (Priority: P3)

**Goal**: Verify all creation and seeding operations are safe to repeat. Handle edge cases gracefully.

**Independent Test**: Run creation and seeding operations multiple times in sequence and verify database state remains correct and consistent after each run.

### Tests for User Story 3 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation (if new behavior needed)**

- [X] T019 [US3] Write idempotency tests in `tests/RentalForge.Api.Tests/Integration/ReferenceDataTests.cs` — tests: calling `EnsureCreatedAsync()` twice doesn't duplicate reference data (109 countries, not 218)
- [X] T020 [US3] Write idempotency and edge case tests in `tests/RentalForge.Api.Tests/Integration/DevDataSeederTests.cs` — tests: SeedAsync called twice without force (second returns false, data unchanged), SeedForceAsync called twice (correct data after each), SeedAsync on partially seeded database handles gracefully, SeedForceAsync on partially seeded database cleans and reseeds correctly
- [X] T021 [US3] Verify error handling in `DevDataSeeder` — ensure clear error messages for: database unreachable, schema not applied (no tables), seed data files missing/corrupt. Add error handling tests if behavior needs implementation changes. Per contracts/cli-seed.md error conditions table.
- [X] T022 [US3] Run all tests — verify all idempotency and edge case tests pass (GREEN) and all previous tests remain unaffected

**Checkpoint**: All operations are idempotent and safe. Edge cases handled gracefully. All tests pass.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup

- [X] T023 Run final verification against all success criteria: SC-001 (schema + reference data < 30s), SC-002 (4 reference tables populated, 11 empty), SC-003 (full seed < 60s), SC-004 (all 15 tables correct after seed), SC-005 (repeated operations no errors), SC-006 (progress output visible)
- [X] T024 Run quickstart.md validation — verify all commands in `specs/003-db-schema-seeding/quickstart.md` work as documented
- [X] T025 Run `dotnet build` and `dotnet test` to confirm zero warnings and zero failures

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **US1 (Phase 2)**: Depends on Setup — BLOCKS US2 and US3 (schema + reference data required)
- **US2 (Phase 3)**: Depends on US1 completion (needs reference data in DB for FK integrity during seeding)
- **US3 (Phase 4)**: Depends on US1 + US2 completion (validates idempotency of both mechanisms)
- **Polish (Phase 5)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1 (Setup) ──→ Phase 2 (US1: Schema + Reference Data)
                         │
                         ├──→ Phase 3 (US2: Dev Seeder) ──→ Phase 4 (US3: Idempotency)
                         │                                         │
                         └─────────────────────────────────────────┘──→ Phase 5 (Polish)
```

- **US1 (P1)**: Can start after Setup. No dependencies on other stories. **This is the MVP.**
- **US2 (P2)**: Depends on US1. Requires schema + reference data to exist for FK integrity during seeding.
- **US3 (P3)**: Depends on US1 + US2. Tests idempotency of both schema creation and dev seeding.

### Within Each User Story

- Tests MUST be written and MUST FAIL before implementation (constitution: Test-First NON-NEGOTIABLE)
- Data/model tasks before service/logic tasks
- Service/logic tasks before CLI integration tasks
- All tests GREEN before advancing to next phase

### Parallel Opportunities

**Within Phase 2 (US1):**
```
T003 (LanguageData.cs) ─┐
T004 (CategoryData.cs) ──┼── All [P] — different files, no dependencies
T005 (CountryData.cs) ──┤
T006 (CityData.cs) ─────┘
     │
     └──→ T007 (HasData in DvdrentalContext) ──→ T008 (Generate migration) ──→ T009 (Run tests)
```

**Within Phase 3 (US2):**
```
T010 (actors/addresses/films JSON) ──┐
T011 (staff/stores/customers JSON) ──┼── All [P] — independent psql exports
T012 (film_actors/categories/inv) ───┤
T013 (rentals/payments JSON) ────────┘
     │
     └──→ T014 (EmbeddedResource) ──→ T015 (Tests RED) ──→ T016 (DevDataSeeder) ──→ T017 (Program.cs CLI) ──→ T018 (Tests GREEN)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001)
2. Complete Phase 2: US1 — Schema + Reference Data (T002-T009)
3. **STOP and VALIDATE**: Schema created, 4 reference tables populated, 11 empty. All tests pass.
4. This is independently valuable — developers can create fresh databases with reference data.

### Incremental Delivery

1. **US1 complete** → Schema + reference data working (MVP)
2. **US2 complete** → Full dev seeding via CLI (Dev experience)
3. **US3 complete** → Idempotency guaranteed (Robustness)
4. **Polish** → Verification and cleanup

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks in same phase
- [Story] label maps task to specific user story for traceability
- Reference data values sourced from original dvdrental database (see research.md for exact row counts)
- Staff ↔ Store circular FK handled via `SET session_replication_role = 'replica'` during dev seeding (see research.md Decision 4)
- JSON seed data files are extracted once from live DB and committed to repo (~1-2 MB total)
- TestWebAppFactory uses `EnsureCreatedAsync()` which automatically applies HasData — no test infrastructure changes needed
- Commit after each completed task or logical group per constitution
