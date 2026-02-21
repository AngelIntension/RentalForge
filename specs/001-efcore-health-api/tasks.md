# Tasks: EF Core Scaffold and Health API

**Input**: Design documents from `/specs/001-efcore-health-api/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Constitution Principle II (Test-First) mandates TDD. Integration tests are written before implementation in each user story phase.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **API project**: `src/RentalForge.Api/`
- **Test project**: `tests/RentalForge.Api.Tests/`
- **Solution**: `RentalForge.sln` at repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create solution, projects, and install all NuGet dependencies

- [ ] T001 Create solution file `RentalForge.sln` at repository root, API project `src/RentalForge.Api/RentalForge.Api.csproj` (net10.0, web SDK), and test project `tests/RentalForge.Api.Tests/RentalForge.Api.Tests.csproj` (net10.0). Add both projects to the solution. Add project reference from test project to API project.
- [ ] T002 [P] Add NuGet dependencies to `src/RentalForge.Api/RentalForge.Api.csproj`: Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, Microsoft.AspNetCore.OpenApi, Swashbuckle.AspNetCore
- [ ] T003 [P] Add NuGet dependencies to `tests/RentalForge.Api.Tests/RentalForge.Api.Tests.csproj`: xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing, Testcontainers.PostgreSql

**Checkpoint**: `dotnet build` succeeds for the entire solution

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

- [ ] T004 Create `src/RentalForge.Api/appsettings.json` with empty `ConnectionStrings:Dvdrental` placeholder (empty string value). Update `.gitignore` at repo root to include `appsettings.Development.json`, `.env`, and `secrets.json`.
- [ ] T005 Initialize `dotnet user-secrets` for the API project (`dotnet user-secrets init` in `src/RentalForge.Api/`). Document the `dotnet user-secrets set` command in a comment in appsettings.json for developer onboarding.
- [ ] T006 Create minimal `src/RentalForge.Api/Data/DvdrentalContext.cs` with constructor accepting `DbContextOptions<DvdrentalContext>`. No entities yet — scaffolding fills these in during US2.
- [ ] T007 Configure `src/RentalForge.Api/Program.cs`: register DvdrentalContext with Npgsql provider reading `ConnectionStrings:Dvdrental` from configuration, add connection string validation that fails fast at startup with actionable error if missing or empty (FR-008), enable Swagger/OpenAPI with Swagger UI in development, configure structured logging via `ILogger`.
- [ ] T008 Create `tests/RentalForge.Api.Tests/Infrastructure/TestWebAppFactory.cs`: implement `WebApplicationFactory<Program>` with `IAsyncLifetime`, provision `PostgreSqlContainer` via Testcontainers (`postgres:18` image), override DvdrentalContext registration in `ConfigureWebHost` to use container connection string.

**Checkpoint**: `dotnet build` succeeds. `dotnet run --project src/RentalForge.Api` fails fast with actionable error (no connection string set via user-secrets yet). Test factory compiles.

---

## Phase 3: User Story 1 - Database Health Check (Priority: P1) MVP

**Goal**: GET `/health` returns database version and server time (200) or error (503)

**Independent Test**: Send GET to `/health`, verify 200 response contains `databaseVersion` and `serverTime` fields. Send GET to `/health` with unreachable DB, verify 503 with `error` field.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation (Principle II)**

- [ ] T009 [US1] Write failing integration test `HealthEndpoint_ReturnsOk_WhenDatabaseIsReachable` in `tests/RentalForge.Api.Tests/Integration/HealthEndpointTests.cs`: send GET `/health` via TestWebAppFactory HttpClient, assert 200 status, assert response JSON contains non-empty `status` ("healthy"), `databaseVersion` (contains "PostgreSQL"), and `serverTime` (valid ISO 8601 timestamp). Additionally, assert response time is under 2 seconds (SC-001) using Stopwatch or HttpClient timing. Use FluentAssertions. Test class uses `IClassFixture<TestWebAppFactory>`.
- [ ] T010 [US1] Write failing integration test `HealthEndpoint_Returns503_WhenDatabaseIsUnreachable` in `tests/RentalForge.Api.Tests/Integration/HealthEndpointTests.cs`: create a separate WebApplicationFactory that overrides the connection string to an invalid host, send GET `/health`, assert 503 status, assert response JSON contains `status` ("unhealthy") and non-empty `error` field. Additionally, assert response time is under 5 seconds (SC-002) to validate the timeout contract.
- [ ] T011 [US1] Write failing integration test `App_FailsFast_WhenConnectionStringMissing` in `tests/RentalForge.Api.Tests/Integration/HealthEndpointTests.cs`: create a WebApplicationFactory that overrides configuration to set `ConnectionStrings:Dvdrental` to empty string, assert that creating the HttpClient (which starts the host) throws an exception with a message containing "connection string" or "Dvdrental". Validates FR-008 and SC-004.

### Implementation for User Story 1

- [ ] T012 [US1] Create `HealthResponse` immutable record in `src/RentalForge.Api/Endpoints/HealthEndpoint.cs` with properties: `string Status`, `string? DatabaseVersion`, `DateTimeOffset? ServerTime`, `string? Error` (per data-model.md and Principle VI)
- [ ] T013 [US1] Implement GET `/health` endpoint in `src/RentalForge.Api/Endpoints/HealthEndpoint.cs` as a static method mapped in Program.cs: inject DvdrentalContext, execute `SELECT version()` and `SELECT NOW()` via `Database.GetDbConnection()`, return 200 with HealthResponse on success, catch exceptions and return 503 with error message (per contracts/health-endpoint.md). Log connection failures via ILogger<> before returning 503 (Principle V: actionable structured logging for significant operations). Add OpenAPI metadata: operation summary, 200/503 response descriptions with example values (FR-011).
- [ ] T014 [US1] Register the `/health` endpoint in `src/RentalForge.Api/Program.cs` by calling the mapping method from HealthEndpoint.cs. Run `dotnet test` and verify T009, T010, and T011 tests pass.

**Checkpoint**: GET `/health` returns 200 with database version and server time against Testcontainers. Returns 503 with error against unreachable database. App fails fast with empty connection string. All three tests green. Endpoint visible in Swagger UI.

---

## Phase 4: User Story 2 - Scaffolded Data Access Layer (Priority: P2)

**Goal**: All 15 dvdrental tables represented as entity classes with correct column mappings and relationships

**Independent Test**: Build succeeds with all entities. DbContext registers all 15 DbSets. Entity properties match database columns per data-model.md.

### Tests for User Story 2

> **NOTE: Write these tests FIRST. They will fail until the scaffold (T017) populates entity classes and DbContext configuration. This is the TDD-compatible approach for scaffolded code (Principle II — see plan.md Complexity Tracking for justification).**

- [ ] T015 [US2] Write failing integration test `DbContext_RegistersAll15DbSets` in `tests/RentalForge.Api.Tests/Integration/DataLayerTests.cs`: use TestWebAppFactory to resolve DvdrentalContext from the service provider, use reflection to assert exactly 15 `DbSet<>` properties exist on DvdrentalContext. Use FluentAssertions. Test class uses `IClassFixture<TestWebAppFactory>`.
- [ ] T016 [US2] Write failing integration test `DbContext_CanQueryFilmTable` in `tests/RentalForge.Api.Tests/Integration/DataLayerTests.cs`: use TestWebAppFactory to resolve DvdrentalContext from the service provider, execute a `Take(1).ToListAsync()` query on the films DbSet, assert no exception thrown. Use the actual scaffolded DbSet property name for films (likely `Films` — adjust after T017 scaffold completes if different). This validates entity mapping compiles and executes against a real PostgreSQL instance.

### Implementation for User Story 2

- [ ] T017 [US2] Run `dotnet ef dbcontext scaffold` against the dvdrental database into `src/RentalForge.Api/Data/` with `--output-dir Entities` and `--context-dir .` flags: scaffold all 15 tables (actor, address, category, city, country, customer, film, film_actor, film_category, inventory, language, payment, rental, staff, store). Use Fluent API configuration (no `--data-annotations` flag). Connection string provided via command-line parameter (not committed). Merge scaffolded OnModelCreating into existing DvdrentalContext.cs.
- [ ] T018 [US2] Review and adjust scaffolded entities in `src/RentalForge.Api/Data/Entities/`: verify all 15 entity files exist with correct property names and types per data-model.md, verify custom type mappings for MpaaRating enum, NpgsqlTsVector (film.Fulltext), string[] (film.SpecialFeatures), and year domain (film.ReleaseYear as int?). Add XML documentation comments to each entity class describing its purpose and key relationships (Principle V).
- [ ] T019 [US2] Verify DvdrentalContext.cs in `src/RentalForge.Api/Data/` registers all 15 DbSets, all foreign key relationships are configured in OnModelCreating including the staff↔store circular reference, and `dotnet build` succeeds with zero warnings from entity classes. Run `dotnet test` and verify T015 and T016 pass.

**Checkpoint**: All 15 dvdrental entity classes exist, compile, and have XML docs. DbContext has all DbSets and relationship configurations. `dotnet build` clean. T015 and T016 green.

---

## Phase 5: User Story 3 - Automated Integration Test (Priority: P3)

**Goal**: Integration test suite runs against disposable Testcontainers PostgreSQL, passes, and cleans up automatically

**Independent Test**: Run `dotnet test` — container provisions, tests pass, container is removed. Total time under 60 seconds.

### Implementation for User Story 3

- [ ] T020 [US3] Verify TestWebAppFactory in `tests/RentalForge.Api.Tests/Infrastructure/TestWebAppFactory.cs` implements `IAsyncLifetime` correctly: `InitializeAsync` starts the PostgreSQL container, `DisposeAsync` stops and removes it. Verify Ryuk (Testcontainers resource reaper) is active for orphan cleanup. Run `dotnet test` and confirm no residual Docker containers after test completion (`docker ps -a --filter ancestor=postgres:18`). Note: if Docker is unavailable, Testcontainers produces a clear error natively — no custom handling needed (EC-003).
- [ ] T021 [US3] Run full test suite with `dotnet test` and verify: all tests pass (T009, T010, T011, T015, T016), total execution time (including container provisioning and teardown) is under 60 seconds (SC-005), no test depends on the shared development database at localhost:5432.

**Checkpoint**: `dotnet test` passes. Container lifecycle is fully automated. Suite completes under 60 seconds.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup

- [ ] T022 Verify `/health` endpoint appears in Swagger UI at `/swagger` with documented request/response schemas, operation summary "Database health check", and both 200/503 response descriptions (SC-007)
- [ ] T023 Run quickstart.md validation end-to-end: follow every step in `specs/001-efcore-health-api/quickstart.md` from a clean state — build, configure user-secrets, run API, curl `/health`, view Swagger, run tests — and verify each step succeeds as documented

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational phase completion
- **US2 (Phase 4)**: Depends on Foundational phase completion — can run in parallel with US1 but sequentially recommended (US1 validates the DB connection US2's scaffold relies on)
- **US3 (Phase 5)**: Depends on US1 completion (tests verify the /health endpoint)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) — Independent of US1, but recommended after US1 to validate DB connection first
- **User Story 3 (P3)**: Depends on US1 (the test verifies the /health endpoint implemented in US1)

### Within Each User Story

- Tests MUST be written and FAIL before implementation (Principle II)
- Models/records before endpoint logic
- Endpoint implementation before integration verification
- Story complete before moving to next priority

### Parallel Opportunities

- T002 and T003 can run in parallel (different .csproj files)
- T004 and T005 can run in parallel (different concerns)
- T009, T010, and T011 can run in parallel (different test methods, same file)
- T015 and T016 can run in parallel (different test methods, same file)
- T017 is a single scaffold command; T018 and T019 are sequential review steps

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: GET `/health` returns 200 with version/time, 503 on failure, fails fast on missing config. Tests green.
5. Working API ready for demo

### Incremental Delivery

1. Complete Setup + Foundational -> Foundation ready
2. Add User Story 1 -> Test independently -> Working health endpoint (MVP!)
3. Add User Story 2 -> Test + verify build -> All 15 entities scaffolded
4. Add User Story 3 -> Run full suite -> Automated test pipeline validated
5. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- EF entities remain mutable classes (justified framework constraint — tracked in plan.md Complexity Tracking)
- US2 scaffold tests are written before scaffold output exists (justified TDD exception — tracked in plan.md Complexity Tracking)
- HealthResponse MUST be an immutable record (Principle VI)
- All database queries in /health use raw SQL via DbConnection, not EF entity queries
- Scaffold command requires a running dvdrental database at localhost:5432 with user-secrets configured
- Testcontainers uses `postgres:18` image to match the development PostgreSQL version
