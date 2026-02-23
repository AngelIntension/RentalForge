# Tasks: Film CRUD API

**Input**: Design documents from `/specs/006-film-crud/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/films-api.md, quickstart.md

**Tests**: TDD is NON-NEGOTIABLE per constitution v1.8.0. All test tasks are REQUIRED — write tests first (RED), implement to pass (GREEN), then refactor.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `src/RentalForge.Api/`
- **Tests**: `tests/RentalForge.Api.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify existing project structure and entity scaffolding are ready for Film CRUD.

- [ ] T001 Verify Film, FilmActor, FilmCategory, Actor, Category, Language, and Inventory entities are correctly scaffolded in src/RentalForge.Api/Data/Entities/ and mapped in src/RentalForge.Api/Data/DvdrentalContext.cs — confirm navigation properties (Film.FilmActors, Film.FilmCategories, Film.Language, Film.OriginalLanguage), MpaaRating enum mapping, and that `dotnet build` succeeds

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create all shared types, interfaces, stubs, validators, and test infrastructure that ALL user stories depend on.

**CRITICAL**: No user story work can begin until this phase is complete.

### DTOs and Service Interface

- [ ] T002 [P] Create FilmListResponse positional record DTO (int Id, string Title, string? Description, int? ReleaseYear, int LanguageId, int? OriginalLanguageId, short RentalDuration, decimal RentalRate, short? Length, decimal ReplacementCost, string? Rating, string[]? SpecialFeatures, DateTime LastUpdate) in src/RentalForge.Api/Models/FilmListResponse.cs
- [ ] T003 [P] Create FilmDetailResponse positional record DTO — all FilmListResponse fields plus string LanguageName, string? OriginalLanguageName, IReadOnlyList&lt;string&gt; Actors, IReadOnlyList&lt;string&gt; Categories in src/RentalForge.Api/Models/FilmDetailResponse.cs
- [ ] T004 [P] Create CreateFilmRequest record DTO with init-only properties (Title, Description, ReleaseYear, LanguageId, OriginalLanguageId, RentalDuration, RentalRate, Length, ReplacementCost, Rating as MpaaRating? with JsonStringEnumConverter, SpecialFeatures as string[]?) in src/RentalForge.Api/Models/CreateFilmRequest.cs
- [ ] T005 [P] Create UpdateFilmRequest record DTO with same shape and constraints as CreateFilmRequest in src/RentalForge.Api/Models/UpdateFilmRequest.cs
- [ ] T006 Create IFilmService interface with method signatures: GetFilmsAsync(search, category, rating, yearFrom, yearTo, page, pageSize) → Task&lt;PagedResponse&lt;FilmListResponse&gt;&gt;, GetFilmByIdAsync(id) → Task&lt;Result&lt;FilmDetailResponse&gt;&gt;, CreateFilmAsync(request) → Task&lt;Result&lt;FilmDetailResponse&gt;&gt;, UpdateFilmAsync(id, request) → Task&lt;Result&lt;FilmDetailResponse&gt;&gt;, DeleteFilmAsync(id) → Task&lt;Result&gt; in src/RentalForge.Api/Services/IFilmService.cs
- [ ] T007 Create FilmService stub implementing IFilmService (all methods throw NotImplementedException) and register as scoped service via builder.Services.AddScoped&lt;IFilmService, FilmService&gt;() in src/RentalForge.Api/Services/FilmService.cs and src/RentalForge.Api/Program.cs
- [ ] T008 Create FilmsController shell with [ApiController], [Route("api/films")], primary constructor injecting IFilmService, and private InvalidResult() helper method (matching CustomersController pattern) in src/RentalForge.Api/Controllers/FilmsController.cs

### Validators (TDD)

- [ ] T009 [P] Write CreateFilmValidatorTests using AutoFixture with Customize&lt;CreateFilmRequest&gt; for valid defaults — test all rules: Title NotEmpty + MaxLength(255), Description MaxLength(1000) when not null, ReleaseYear InclusiveBetween(1888, currentYear+5) when not null, LanguageId GreaterThan(0), OriginalLanguageId GreaterThan(0) when not null, RentalDuration GreaterThan(0), RentalRate GreaterThan(0), Length GreaterThan(0) when not null, ReplacementCost GreaterThan(0), Rating must be valid MpaaRating when not null (TDD RED) in tests/RentalForge.Api.Tests/Unit/CreateFilmValidatorTests.cs
- [ ] T010 [P] Write UpdateFilmValidatorTests using AutoFixture — same validation rules as CreateFilmValidator (TDD RED) in tests/RentalForge.Api.Tests/Unit/UpdateFilmValidatorTests.cs
- [ ] T011 [P] Implement CreateFilmValidator extending AbstractValidator&lt;CreateFilmRequest&gt; with FluentValidation rules matching all test expectations (TDD GREEN) in src/RentalForge.Api/Validators/CreateFilmValidator.cs
- [ ] T012 [P] Implement UpdateFilmValidator extending AbstractValidator&lt;UpdateFilmRequest&gt; with FluentValidation rules matching all test expectations (TDD GREEN) in src/RentalForge.Api/Validators/UpdateFilmValidator.cs

### Test Infrastructure

- [ ] T013 Create FilmTestHelper static class with SeedTestDataAsync() — seed via EF Core and raw SQL: 2+ languages, 3+ actors, 3+ categories, 5+ films (varied ratings, release years, descriptions), film_actor join rows, film_category join rows; add SeedFilmWithInventoryAsync() for delete-blocking tests; use high ID range (9000+) to avoid collisions with reference data in tests/RentalForge.Api.Tests/Infrastructure/FilmTestHelper.cs

**Checkpoint**: Foundation ready — all types compile, validator unit tests pass, test helper ready. User story implementation can begin.

---

## Phase 3: User Story 1 — Browse and Search Films (Priority: P1) 🎯 MVP

**Goal**: Staff can browse, search, and filter the film catalog with pagination returning lean DTOs.

**Independent Test**: Send GET /api/films with various query parameter combinations; verify filtered, paginated results match expected lean DTO shape.

### Tests (TDD RED)

- [ ] T014 [US1] Write integration tests for GET /api/films in tests/RentalForge.Api.Tests/Integration/FilmEndpointTests.cs — create test class with IClassFixture&lt;TestWebAppFactory&gt;, IAsyncLifetime; use FilmTestHelper.SeedTestDataAsync() in InitializeAsync; cover all acceptance scenarios: default pagination returns films with lean DTO shape (no actor/category names), search by partial title (case-insensitive), search by description keyword, search by actor first/last name, filter by category name (case-insensitive exact match), filter by MPAA rating, filter by yearFrom+yearTo range (inclusive), yearFrom only, yearTo only, combined search+category+rating AND logic, page 2 with pagination metadata (totalCount, totalPages, page, pageSize), empty results return zero totalCount, page &lt; 1 → 400 validation error, pageSize &lt; 1 → 400, pageSize &gt; 100 capped silently, yearFrom &gt; yearTo → 400, default results ordered alphabetically by title (verify items[0].Title &lt;= items[1].Title)

### Implementation (TDD GREEN)

- [ ] T015 [US1] Implement FilmService.GetFilmsAsync() in src/RentalForge.Api/Services/FilmService.cs — inject DvdrentalContext; build IQueryable&lt;Film&gt; pipeline: apply ILike search on title OR description OR actor first/last name (via SelectMany on FilmActors), category filter via join to FilmCategories→Category with ILike exact match, rating filter on Film.Rating enum, yearFrom/yearTo range with >= / <= on ReleaseYear; order by Title; paginate with Skip/Take; project to FilmListResponse (Rating serialized as string via .ToString()); return PagedResponse with TotalCount via CountAsync
- [ ] T016 [US1] Implement FilmsController.GetFilms() action in src/RentalForge.Api/Controllers/FilmsController.cs — [HttpGet] with [FromQuery] parameters: string? search, string? category, string? rating, int? yearFrom, int? yearTo, int page = 1, int pageSize = 10; validate page >= 1, pageSize >= 1, yearFrom <= yearTo (when both provided) using aggregated ValidationProblemDetails; cap pageSize = Math.Min(pageSize, 100); delegate to service; return Ok(result)

**Checkpoint**: GET /api/films fully functional with search, filters, and pagination. All US1 integration tests pass.

---

## Phase 4: User Story 2 — View Film Details (Priority: P1)

**Goal**: Staff can view full film details including actor names, category names, and language name in a flat DTO.

**Independent Test**: Send GET /api/films/{id} and verify all fields including flat related data; verify 404 for non-existent film.

### Tests (TDD RED)

- [ ] T017 [US2] Write integration tests for GET /api/films/{id} in tests/RentalForge.Api.Tests/Integration/FilmEndpointTests.cs — cover: film exists → 200 with FilmDetailResponse containing all core fields plus LanguageName (trimmed), OriginalLanguageName when set, Actors as list of "FirstName LastName" strings, Categories as list of category name strings; 404 for non-existent film ID

### Implementation (TDD GREEN)

- [ ] T018 [US2] Implement FilmService.GetFilmByIdAsync() in src/RentalForge.Api/Services/FilmService.cs — query film by ID using Select() projection to load Language.Name, OriginalLanguage.Name (trimmed, since char(20) column), FilmActors→Actor mapped to "FirstName LastName", FilmCategories→Category.Name; map to FilmDetailResponse; return Result.NotFound() if film not found
- [ ] T019 [US2] Implement FilmsController.GetFilm() action in src/RentalForge.Api/Controllers/FilmsController.cs — [HttpGet("{id}")] with int id parameter; delegate to service; return Ok(result.Value) for success, NotFound() for not found via result.Status switch

**Checkpoint**: GET /api/films/{id} returns full flat detail with related data. All US1 and US2 tests pass.

---

## Phase 5: User Story 3 — Add a New Film (Priority: P2)

**Goal**: Staff can create new films with full validation and aggregated error reporting.

**Independent Test**: POST /api/films with valid/invalid payloads; verify 201 with Location header and FilmDetailResponse, or 400 with aggregated errors.

### Tests (TDD RED)

- [ ] T020 [US3] Write integration tests for POST /api/films in tests/RentalForge.Api.Tests/Integration/FilmEndpointTests.cs — cover: create with required fields only → 201 + Location header + FilmDetailResponse body with generated ID, create with all optional fields including rating and special features, missing title → 400, blank title → 400, invalid language ID (non-existent FK) → 400 with "Language with ID {id} does not exist.", invalid original language ID → 400, invalid rating value → 400, multiple simultaneous validation errors aggregated in single 400 response (not early-return), rating accepts string "PG-13" and numeric 2 in request body, created film appears in GET list

### Implementation (TDD GREEN)

- [ ] T021 [US3] Implement FilmService.CreateFilmAsync() in src/RentalForge.Api/Services/FilmService.cs — inject IValidator&lt;CreateFilmRequest&gt;; run validator and collect errors via .AsErrors(); check LanguageId exists via db.Languages.AnyAsync (add error if not); check OriginalLanguageId exists if provided (add error if not); if any errors, return Result.Invalid(allErrors); map request to Film entity (set LastUpdate = DateTime.UtcNow); add to db and SaveChangesAsync; reload full detail via GetFilmByIdAsync pattern; return Result.Created(detail)
- [ ] T022 [US3] Implement FilmsController.CreateFilm() action in src/RentalForge.Api/Controllers/FilmsController.cs — [HttpPost] accepting [FromBody] CreateFilmRequest; delegate to service; return CreatedAtAction(nameof(GetFilm), new { id = result.Value.Id }, result.Value) for Created, InvalidResult() for Invalid via result.Status switch

**Checkpoint**: POST /api/films creates films with full validation. All US1–US3 tests pass.

---

## Phase 6: User Story 4 — Update Film Information (Priority: P2)

**Goal**: Staff can update existing film metadata with full validation and aggregated error reporting.

**Independent Test**: PUT /api/films/{id} with valid/invalid payloads; verify 200 with updated FilmDetailResponse, 400 for validation errors, 404 for non-existent film.

### Tests (TDD RED)

- [ ] T023 [US4] Write integration tests for PUT /api/films/{id} in tests/RentalForge.Api.Tests/Integration/FilmEndpointTests.cs — cover: update title and rental rate → 200 + updated FilmDetailResponse, update all fields including optional ones, validation errors (blank title, negative rental rate) → 400, non-existent film → 404, invalid language ID → 400 with FK error message, multiple validation errors aggregated

### Implementation (TDD GREEN)

- [ ] T024 [US4] Implement FilmService.UpdateFilmAsync() in src/RentalForge.Api/Services/FilmService.cs — inject IValidator&lt;UpdateFilmRequest&gt;; find film by ID (return Result.NotFound() if not found); run validator and collect .AsErrors(); check LanguageId FK existence; check OriginalLanguageId FK existence if provided; aggregate errors; if any, return Result.Invalid(allErrors); map request fields onto entity (update LastUpdate = DateTime.UtcNow); SaveChangesAsync; reload detail; return Result.Success(detail)
- [ ] T025 [US4] Implement FilmsController.UpdateFilm() action in src/RentalForge.Api/Controllers/FilmsController.cs — [HttpPut("{id}")] accepting int id and [FromBody] UpdateFilmRequest; delegate to service; return Ok(result.Value) for success, NotFound() for not found, InvalidResult() for validation errors via result.Status switch

**Checkpoint**: PUT /api/films/{id} updates films with full validation. All US1–US4 tests pass.

---

## Phase 7: User Story 5 — Remove a Film (Priority: P3)

**Goal**: Staff can permanently delete films that have no inventory records; films with inventory are protected.

**Independent Test**: DELETE /api/films/{id}; verify 204 for clean delete with cascade on joins, 409 for inventory conflict, 404 for non-existent film.

### Tests (TDD RED)

- [ ] T026 [US5] Write integration tests for DELETE /api/films/{id} in tests/RentalForge.Api.Tests/Integration/FilmEndpointTests.cs — cover: delete film with no inventory → 204, film_actor and film_category join rows also removed; delete film with inventory records → 409 Conflict with detail message "Cannot delete film with ID {id} because it has associated inventory records."; non-existent film → 404; after successful delete, GET /api/films/{id} returns 404 and GET /api/films?search= does not include deleted film (use FilmTestHelper.SeedFilmWithInventoryAsync for inventory scenario)

### Implementation (TDD GREEN)

- [ ] T027 [US5] Implement FilmService.DeleteFilmAsync() in src/RentalForge.Api/Services/FilmService.cs — find film by ID (return Result.NotFound() if not found); check db.Inventories.AnyAsync(i => i.FilmId == id) (return Result.Conflict("Cannot delete film with ID {id} because it has associated inventory records.") if true); remove film (EF Core cascades film_actor/film_category deletes); SaveChangesAsync; return Result.NoContent()
- [ ] T028 [US5] Implement FilmsController.DeleteFilm() action in src/RentalForge.Api/Controllers/FilmsController.cs — [HttpDelete("{id}")] accepting int id; delegate to service; return NoContent() for success, NotFound() for not found, Conflict with ProblemDetails for conflict via result.Status switch

**Checkpoint**: DELETE /api/films/{id} works with inventory protection. All US1–US5 tests pass.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, observability, and final validation across all endpoints.

- [ ] T029 [P] Add XML documentation comments and SwaggerOperation attributes (operationIds: ListFilms, GetFilm, CreateFilm, UpdateFilm, DeleteFilm) with ProducesResponseType annotations to all FilmsController actions in src/RentalForge.Api/Controllers/FilmsController.cs
- [ ] T030 [P] Add structured logging via ILogger&lt;FilmService&gt; for film created (LogInformation with film ID and title), film updated, film deleted, delete blocked by inventory, and search queries in src/RentalForge.Api/Services/FilmService.cs
- [ ] T031 Run full test suite (`dotnet test`) and verify all tests pass (existing Customer tests + new Film tests) with zero warnings from `dotnet build`
- [ ] T032 Run quickstart.md validation checklist: verify Swagger UI shows all 5 Film endpoints with correct schemas, list returns lean DTOs, detail returns flat related data, rating accepts both string and numeric, delete returns 409 for films with inventory, all validation errors are aggregated, spot-check list/search/detail response times are under 1 second against seeded data (SC-001/SC-002/SC-003)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — verify existing scaffolding
- **Foundational (Phase 2)**: Depends on Phase 1 — creates all shared types and test infrastructure
- **US1 (Phase 3)**: Depends on Phase 2 — implements list/search/filter endpoint
- **US2 (Phase 4)**: Depends on Phase 2 — implements detail endpoint (independent of US1)
- **US3 (Phase 5)**: Depends on Phase 2 — implements create endpoint (independent of US1/US2)
- **US4 (Phase 6)**: Depends on Phase 2 — implements update endpoint (independent of US1–US3)
- **US5 (Phase 7)**: Depends on Phase 2 — implements delete endpoint (independent of US1–US4)
- **Polish (Phase 8)**: Depends on all user stories (Phases 3–7) being complete

### User Story Independence

All user stories depend only on Phase 2 (foundational). They can be implemented in parallel or in priority order:

- **US1 + US2 (both P1)**: Can run in parallel — different service methods, different controller actions
- **US3 + US4 (both P2)**: Can run in parallel — create and update are independent
- **US5 (P3)**: Can run in parallel with any other story

### Within Each User Story (TDD Cycle)

1. Write integration tests → tests compile but **FAIL** (RED)
2. Implement service method → business logic passes tests
3. Implement controller action → HTTP layer wires up (GREEN)
4. Refactor if needed — all tests still pass

### Parallel Opportunities

- **Phase 2 DTOs**: T002, T003, T004, T005 — all different files, no dependencies
- **Phase 2 Validator Tests**: T009, T010 — different test files
- **Phase 2 Validator Impls**: T011, T012 — different source files (after their respective test)
- **After Phase 2**: US1–US5 all independently implementable
- **Phase 8**: T029, T030 — docs and logging in different files

---

## Parallel Example: Phase 2 Foundation

```bash
# Launch all DTOs in parallel (T002–T005):
Task: "Create FilmListResponse in src/RentalForge.Api/Models/FilmListResponse.cs"
Task: "Create FilmDetailResponse in src/RentalForge.Api/Models/FilmDetailResponse.cs"
Task: "Create CreateFilmRequest in src/RentalForge.Api/Models/CreateFilmRequest.cs"
Task: "Create UpdateFilmRequest in src/RentalForge.Api/Models/UpdateFilmRequest.cs"

# Then launch validator tests in parallel (T009–T010):
Task: "Write CreateFilmValidatorTests in tests/.../Unit/CreateFilmValidatorTests.cs"
Task: "Write UpdateFilmValidatorTests in tests/.../Unit/UpdateFilmValidatorTests.cs"

# Then launch validator implementations in parallel (T011–T012):
Task: "Implement CreateFilmValidator in src/.../Validators/CreateFilmValidator.cs"
Task: "Implement UpdateFilmValidator in src/.../Validators/UpdateFilmValidator.cs"
```

## Parallel Example: User Stories (after Phase 2)

```bash
# All user stories can start simultaneously after Phase 2:
Agent A: Phase 3 (US1 — Browse/Search) — T014 → T015 → T016
Agent B: Phase 4 (US2 — View Details) — T017 → T018 → T019
Agent C: Phase 5 (US3 — Create) — T020 → T021 → T022
Agent D: Phase 6 (US4 — Update) — T023 → T024 → T025
Agent E: Phase 7 (US5 — Delete) — T026 → T027 → T028
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2)

1. Complete Phase 1: Verify entity scaffolding
2. Complete Phase 2: Foundation (DTOs, validators + tests, service stub, controller shell, test helper)
3. Complete Phase 3: US1 — Browse and Search Films (TDD)
4. Complete Phase 4: US2 — View Film Details (TDD)
5. **STOP and VALIDATE**: Both P1 stories independently testable
6. Deploy/demo — staff can search and view films

### Incremental Delivery

1. Phase 1 + 2 → Foundation ready
2. Add US1 → Test → Deploy (staff can browse/search catalog)
3. Add US2 → Test → Deploy (staff can view film details)
4. Add US3 + US4 → Test → Deploy (staff can manage film catalog)
5. Add US5 → Test → Deploy (staff can clean up catalog)
6. Phase 8 → Polish → Final validation

---

## Notes

- [P] tasks = different files, no dependencies on each other
- [USn] label maps task to specific user story for traceability
- TDD is NON-NEGOTIABLE: write tests first (RED), implement (GREEN), refactor
- Validators are tested with FluentValidation.TestHelper (TestValidate + ShouldHaveValidationErrorFor)
- Integration tests use Testcontainers.PostgreSql via shared TestWebAppFactory (IClassFixture)
- AutoFixture generates anonymous test data; Customize&lt;T&gt; for domain-specific defaults
- All service methods return Ardalis.Result&lt;T&gt; or Result — no exceptions for expected outcomes
- All validation errors MUST be aggregated (FluentValidation .AsErrors() + FK checks combined)
- DTOs follow constitution v1.8.0: lean list (IDs only), flat detail (inlined names), enum string+numeric
- MpaaRating enum needs JsonStringEnumConverter on request DTO properties
- Language.Name is char(20) — trim trailing spaces when mapping to response
- Film.Rating is PostgreSQL enum mpaa_rating — map via EF Core enum converter
- Commit after each task or logical group completes
- Stop at any checkpoint to validate story independently
