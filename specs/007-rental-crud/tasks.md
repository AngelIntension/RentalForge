# Tasks: Rental CRUD API

**Input**: Design documents from `/specs/007-rental-crud/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/rentals-api.md, quickstart.md

**Tests**: TDD is NON-NEGOTIABLE per constitution v1.9.0. All test tasks are REQUIRED — write tests first (RED), implement to pass (GREEN), then refactor.

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

**Purpose**: Verify existing project structure and entity scaffolding are ready for Rental CRUD.

- [ ] T001 Verify Rental, Inventory, Customer, Staff, Film, Store, and Payment entities are correctly scaffolded in src/RentalForge.Api/Data/Entities/ and mapped in src/RentalForge.Api/Data/DvdrentalContext.cs — confirm navigation properties (Rental.Inventory, Rental.Customer, Rental.Staff, Rental.Payments, Inventory.Film, Inventory.Store, Inventory.Rentals, Customer.Activebool, Staff.Active), FK column types (customer_id smallint, staff_id smallint), and that `dotnet build` succeeds

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create all shared types, interfaces, stubs, validators, and test infrastructure that ALL user stories depend on.

**CRITICAL**: No user story work can begin until this phase is complete.

### DTOs and Service Interface

- [ ] T002 [P] Create RentalListResponse positional record DTO (int Id, DateTime RentalDate, DateTime? ReturnDate, int InventoryId, int CustomerId, int StaffId, DateTime LastUpdate) in src/RentalForge.Api/Models/RentalListResponse.cs
- [ ] T003 [P] Create RentalDetailResponse positional record DTO (int Id, DateTime RentalDate, DateTime? ReturnDate, int InventoryId, int FilmId, string FilmTitle, int StoreId, int CustomerId, string CustomerFirstName, string CustomerLastName, int StaffId, string StaffFirstName, string StaffLastName, DateTime LastUpdate) in src/RentalForge.Api/Models/RentalDetailResponse.cs
- [ ] T004 [P] Create CreateRentalRequest record DTO with init-only properties (int FilmId, int StoreId, int CustomerId, int StaffId) in src/RentalForge.Api/Models/CreateRentalRequest.cs
- [ ] T005 Create IRentalService interface with method signatures: GetRentalsAsync(customerId?, activeOnly, page, pageSize) → Task&lt;PagedResponse&lt;RentalListResponse&gt;&gt;, GetRentalByIdAsync(id) → Task&lt;Result&lt;RentalDetailResponse&gt;&gt;, CreateRentalAsync(request) → Task&lt;Result&lt;RentalDetailResponse&gt;&gt;, ReturnRentalAsync(id) → Task&lt;Result&lt;RentalDetailResponse&gt;&gt;, DeleteRentalAsync(id) → Task&lt;Result&gt; in src/RentalForge.Api/Services/IRentalService.cs
- [ ] T006 Create RentalService stub implementing IRentalService (all methods throw NotImplementedException) and register as scoped service via builder.Services.AddScoped&lt;IRentalService, RentalService&gt;() in src/RentalForge.Api/Services/RentalService.cs and src/RentalForge.Api/Program.cs
- [ ] T007 Create RentalsController shell with [ApiController], [Route("api/rentals")], primary constructor injecting IRentalService, and private InvalidResult() helper method (matching CustomersController/FilmsController pattern) in src/RentalForge.Api/Controllers/RentalsController.cs

### Validator (TDD)

- [ ] T008 [P] Write CreateRentalValidatorTests using AutoFixture with Customize&lt;CreateRentalRequest&gt; for valid defaults — test all rules: FilmId GreaterThan(0), StoreId GreaterThan(0), CustomerId GreaterThan(0), StaffId GreaterThan(0); test valid request passes, each field at 0 and negative values fails with appropriate error (TDD RED) in tests/RentalForge.Api.Tests/Unit/CreateRentalValidatorTests.cs
- [ ] T009 [P] Implement CreateRentalValidator extending AbstractValidator&lt;CreateRentalRequest&gt; with FluentValidation rules matching all test expectations (TDD GREEN) in src/RentalForge.Api/Validators/CreateRentalValidator.cs

### Test Infrastructure

- [ ] T010 Create RentalTestHelper static class with methods: SeedTestDataAsync() — seed via raw SQL with session_replication_role='replica': 2 stores, 2 staff (1 active + 1 inactive), 3+ customers (active + inactive), 2+ films, 1+ language, inventory records across stores (some with active rentals, some available), rental records (active + returned), payment records for delete-blocking tests; use high ID range (9000+) to avoid collisions; provide SeedRentalWithPaymentAsync() for delete tests and SeedAvailableInventoryAsync() for create tests in tests/RentalForge.Api.Tests/Infrastructure/RentalTestHelper.cs

**Checkpoint**: Foundation ready — all types compile, validator unit tests pass, test helper ready. User story implementation can begin.

---

## Phase 3: User Story 1 — Rent a Film (Priority: P1) 🎯 MVP

**Goal**: Staff can create a new rental by specifying filmId + storeId + customerId + staffId; the system resolves an available inventory copy and returns full rental details.

**Independent Test**: POST /api/rentals with valid/invalid payloads; verify 201 with Location header and RentalDetailResponse (including resolved inventoryId and flat display fields), or 400 with aggregated validation/business-rule errors.

### Tests (TDD RED)

- [ ] T011 [US1] Write integration tests for POST /api/rentals in tests/RentalForge.Api.Tests/Integration/RentalEndpointTests.cs — create test class with IClassFixture&lt;TestWebAppFactory&gt;, IAsyncLifetime; use RentalTestHelper.SeedTestDataAsync() in InitializeAsync; cover all acceptance scenarios: valid create → 201 + Location header + RentalDetailResponse body with resolved inventoryId, filmId, filmTitle, storeId, customerFirstName/LastName, staffFirstName/LastName, rentalDate set to approximately now, returnDate is null; film not stocked at store → 400 with "Film '{title}' is not stocked at store {storeId}"; all copies rented out → 400 with "All copies of film '{title}' at store {storeId} are currently rented out"; non-existent filmId → 400; non-existent storeId → 400; non-existent/inactive customerId → 400; non-existent/inactive staffId → 400; multiple simultaneous validation errors aggregated in single 400 response; filmId=0 → 400 (validator); deterministic inventory selection (lowest inventoryId); created rental appears in GET list; created rental consumes inventory copy (second create for same film+store uses different inventory or fails if none left)

### Implementation (TDD GREEN)

- [ ] T012 [US1] Implement RentalService.CreateRentalAsync() in src/RentalForge.Api/Services/RentalService.cs — inject DvdrentalContext, ILogger&lt;RentalService&gt;, IValidator&lt;CreateRentalRequest&gt;; run validator and collect errors via .AsErrors(); check FilmId exists via db.Films.AnyAsync (add error with film ID if not); check StoreId exists via db.Stores.AnyAsync (add error if not); check CustomerId exists and Activebool==true via db.Customers (add error "Customer with ID {id} does not exist or is inactive." if not); check StaffId exists and Active==true via db.Staff (add error if not); if any errors so far, return Result.Invalid(allErrors); query available inventory: db.Inventories.Where(i => i.FilmId == filmId && i.StoreId == storeId) and filter to copies where !i.Rentals.Any(r => r.ReturnDate == null) ordered by i.InventoryId; if no inventory records at all for film+store → add error "Film '{title}' is not stocked at store {storeId}." and return Invalid; if inventory exists but none available → add error "All copies of film '{title}' at store {storeId} are currently rented out." and return Invalid; create Rental entity (RentalDate=DateTime.UtcNow, InventoryId=available.InventoryId, CustomerId, StaffId, LastUpdate=DateTime.UtcNow); add and SaveChangesAsync; build RentalDetailResponse via projection (include Inventory.Film.Title, Inventory.StoreId, Customer.FirstName/LastName, Staff.FirstName/LastName); log creation; return Result.Created(detail)
- [ ] T013 [US1] Implement RentalsController.CreateRental() action in src/RentalForge.Api/Controllers/RentalsController.cs — [HttpPost] accepting [FromBody] CreateRentalRequest; delegate to service; return CreatedAtAction(nameof(GetRental), new { id = result.Value.Id }, result.Value) for Created, InvalidResult(result.ValidationErrors) for Invalid via result.Status switch

**Checkpoint**: POST /api/rentals fully functional with inventory resolution and aggregated validation. All US1 integration tests pass.

---

## Phase 4: User Story 2 — List and Filter Rentals (Priority: P1)

**Goal**: Staff can browse rentals with customerId and activeOnly filters, with pagination and lean DTO responses.

**Independent Test**: Send GET /api/rentals with various filter/pagination combinations; verify filtered, paginated results match expected lean DTO shape.

### Tests (TDD RED)

- [ ] T014 [US2] Write integration tests for GET /api/rentals in tests/RentalForge.Api.Tests/Integration/RentalEndpointTests.cs — cover all acceptance scenarios: default pagination returns rentals with lean DTO shape (IDs only, no names), filter by customerId returns only that customer's rentals, activeOnly=true returns only rentals with null returnDate, activeOnly=false returns all rentals (same as omitted), combined customerId + activeOnly returns intersection, page 2 with pagination metadata (totalCount, totalPages, page, pageSize), empty results return zero totalCount, page &lt; 1 → 400, pageSize &lt; 1 → 400, pageSize &gt; 100 capped silently, customerId for non-existent customer returns empty list (not error), page exceeding total pages returns empty items with correct totalCount and totalPages, default sort order is rental date descending (verify items[0].RentalDate &gt;= items[1].RentalDate)

### Implementation (TDD GREEN)

- [ ] T015 [US2] Implement RentalService.GetRentalsAsync() in src/RentalForge.Api/Services/RentalService.cs — build IQueryable&lt;Rental&gt; pipeline: if customerId provided, filter Where(r => r.CustomerId == customerId); if activeOnly is true, filter Where(r => r.ReturnDate == null); order by RentalDate descending then by RentalId descending; count total; paginate with Skip/Take; project to RentalListResponse; return PagedResponse
- [ ] T016 [US2] Implement RentalsController.GetRentals() action in src/RentalForge.Api/Controllers/RentalsController.cs — [HttpGet] with [FromQuery] parameters: int? customerId, bool activeOnly = false, int page = 1, int pageSize = 10; validate page &gt;= 1 and pageSize &gt;= 1 using aggregated ValidationProblemDetails; cap pageSize = Math.Min(pageSize, 100); delegate to service; return Ok(result)

**Checkpoint**: GET /api/rentals fully functional with filters and pagination. All US1 and US2 integration tests pass.

---

## Phase 5: User Story 3 — View Rental Details (Priority: P1)

**Goal**: Staff can view full rental details including flat display fields for customer name, film title, and staff name.

**Independent Test**: Send GET /api/rentals/{id} and verify all fields including flat related data; verify 404 for non-existent rental.

### Tests (TDD RED)

- [ ] T017 [US3] Write integration tests for GET /api/rentals/{id} in tests/RentalForge.Api.Tests/Integration/RentalEndpointTests.cs — cover: rental exists → 200 with RentalDetailResponse containing all fields: id, rentalDate, returnDate, inventoryId, filmId, filmTitle (from Inventory→Film), storeId (from Inventory→Store), customerId, customerFirstName, customerLastName, staffId, staffFirstName, staffLastName, lastUpdate; active rental has returnDate == null; returned rental has returnDate set; 404 for non-existent rental ID

### Implementation (TDD GREEN)

- [ ] T018 [US3] Implement RentalService.GetRentalByIdAsync() in src/RentalForge.Api/Services/RentalService.cs — query rental by ID using Select() projection to load Inventory.Film.Title, Inventory.StoreId, Inventory.FilmId, Customer.FirstName/LastName, Staff.FirstName/LastName; map to RentalDetailResponse; return Result.NotFound() if rental not found, Result.Success(detail) otherwise
- [ ] T019 [US3] Implement RentalsController.GetRental() action in src/RentalForge.Api/Controllers/RentalsController.cs — [HttpGet("{id:int}")] with int id parameter; delegate to service; return Ok(result.Value) for success, NotFound() for not found via result.Status switch

**Checkpoint**: GET /api/rentals/{id} returns full flat detail with related data. All US1–US3 tests pass.

---

## Phase 6: User Story 4 — Return a Rental (Priority: P2)

**Goal**: Staff can process a rental return which sets the return date and makes the inventory copy available for new rentals.

**Independent Test**: PUT /api/rentals/{id}/return on an active rental; verify return date is set and detail response reflects the change; verify already-returned rental is rejected.

### Tests (TDD RED)

- [ ] T020 [US4] Write integration tests for PUT /api/rentals/{id}/return in tests/RentalForge.Api.Tests/Integration/RentalEndpointTests.cs — cover: active rental → 200 with RentalDetailResponse containing returnDate set to approximately now and all flat display fields; already-returned rental → 400 with "Rental with ID {id} has already been returned."; non-existent rental → 404; after return, the inventory copy becomes available (create new rental for same film+store succeeds); after return, GET /api/rentals/{id} shows returnDate set

### Implementation (TDD GREEN)

- [ ] T021 [US4] Implement RentalService.ReturnRentalAsync() in src/RentalForge.Api/Services/RentalService.cs — find rental by ID with Include(r => r.Inventory.Film), Include(r => r.Customer), Include(r => r.Staff), Include(r => r.Inventory.Store); return Result.NotFound() if not found; if ReturnDate is already set, return Result.Invalid with error "Rental with ID {id} has already been returned." on identifier "rentalId"; set ReturnDate = DateTime.UtcNow, LastUpdate = DateTime.UtcNow; SaveChangesAsync; build RentalDetailResponse; log return; return Result.Success(detail)
- [ ] T022 [US4] Implement RentalsController.ReturnRental() action in src/RentalForge.Api/Controllers/RentalsController.cs — [HttpPut("{id:int}/return")] with int id parameter; no request body; delegate to service; return Ok(result.Value) for success, NotFound() for not found, InvalidResult(result.ValidationErrors) for Invalid via result.Status switch

**Checkpoint**: PUT /api/rentals/{id}/return works with double-return rejection. All US1–US4 tests pass.

---

## Phase 7: User Story 5 — Delete a Rental (Priority: P3)

**Goal**: Administrators can permanently delete a rental with no payments; rentals with payments are protected by conflict response.

**Independent Test**: DELETE /api/rentals/{id}; verify 204 for clean delete, 409 for payment conflict, 404 for non-existent rental.

### Tests (TDD RED)

- [ ] T023 [US5] Write integration tests for DELETE /api/rentals/{id} in tests/RentalForge.Api.Tests/Integration/RentalEndpointTests.cs — cover: delete rental with no payments → 204; delete rental with payment records → 409 Conflict with detail message "Cannot delete rental with ID {id} because it has associated payment records."; non-existent rental → 404; after successful delete, GET /api/rentals/{id} returns 404 and GET /api/rentals does not include deleted rental (use RentalTestHelper.SeedRentalWithPaymentAsync for payment scenario)

### Implementation (TDD GREEN)

- [ ] T024 [US5] Implement RentalService.DeleteRentalAsync() in src/RentalForge.Api/Services/RentalService.cs — find rental by ID (return Result.NotFound() if not found); check db.Payments.AnyAsync(p => p.RentalId == id) (return Result.Conflict("Cannot delete rental with ID {id} because it has associated payment records.") if true); remove rental; SaveChangesAsync; log deletion; return Result.NoContent()
- [ ] T025 [US5] Implement RentalsController.DeleteRental() action in src/RentalForge.Api/Controllers/RentalsController.cs — [HttpDelete("{id:int}")] accepting int id; delegate to service; return NoContent() for success, NotFound() for not found, Conflict with ProblemDetails for conflict via result.Status switch

**Checkpoint**: DELETE /api/rentals/{id} works with payment protection. All US1–US5 tests pass.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, observability, and final validation across all endpoints.

- [ ] T026 [P] Add XML documentation comments and ProducesResponseType annotations to all RentalsController actions (5 endpoints: GetRentals, GetRental, CreateRental, ReturnRental, DeleteRental) in src/RentalForge.Api/Controllers/RentalsController.cs
- [ ] T027 [P] Add structured logging via ILogger&lt;RentalService&gt; for rental created (LogInformation with rental ID, film title, customer name), rental returned, rental deleted, delete blocked by payments, inventory resolution results, and no-availability scenarios in src/RentalForge.Api/Services/RentalService.cs
- [ ] T028 Run full test suite (`dotnet test`) and verify all tests pass (existing Customer + Film tests + new Rental tests) with zero warnings from `dotnet build`
- [ ] T029 Run quickstart.md validation checklist: verify Swagger UI shows all 5 Rental endpoints with correct schemas, list returns lean DTOs (IDs only), detail returns flat related data (customer name, film title, staff name), create with filmId+storeId resolves inventory, create with unavailable film returns specific error (not stocked vs all rented), return endpoint sets return date and rejects double-return, delete returns 409 for rentals with payments, all validation errors are aggregated, spot-check list/detail response times under 1 second (SC-003)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — verify existing scaffolding
- **Foundational (Phase 2)**: Depends on Phase 1 — creates all shared types and test infrastructure
- **US1 (Phase 3)**: Depends on Phase 2 — implements create endpoint with inventory resolution
- **US2 (Phase 4)**: Depends on Phase 2 — implements list/filter endpoint (independent of US1)
- **US3 (Phase 5)**: Depends on Phase 2 — implements detail endpoint (independent of US1/US2)
- **US4 (Phase 6)**: Depends on Phase 2 — implements return endpoint (independent of US1–US3)
- **US5 (Phase 7)**: Depends on Phase 2 — implements delete endpoint (independent of US1–US4)
- **Polish (Phase 8)**: Depends on all user stories (Phases 3–7) being complete

### User Story Independence

All user stories depend only on Phase 2 (foundational). They can be implemented in parallel or in priority order:

- **US1 + US2 + US3 (all P1)**: Can run in parallel — different service methods, different controller actions
- **US4 (P2)**: Can run in parallel with any P1 story
- **US5 (P3)**: Can run in parallel with any other story

### Within Each User Story (TDD Cycle)

1. Write integration tests → tests compile but **FAIL** (RED)
2. Implement service method → business logic passes tests
3. Implement controller action → HTTP layer wires up (GREEN)
4. Refactor if needed — all tests still pass

### Parallel Opportunities

- **Phase 2 DTOs**: T002, T003, T004 — all different files, no dependencies
- **Phase 2 Validator**: T008 (test) then T009 (impl) — sequential within pair
- **After Phase 2**: US1–US5 all independently implementable
- **Phase 8**: T026, T027 — docs and logging in different files

---

## Parallel Example: Phase 2 Foundation

```bash
# Launch all DTOs in parallel (T002–T004):
Task: "Create RentalListResponse in src/RentalForge.Api/Models/RentalListResponse.cs"
Task: "Create RentalDetailResponse in src/RentalForge.Api/Models/RentalDetailResponse.cs"
Task: "Create CreateRentalRequest in src/RentalForge.Api/Models/CreateRentalRequest.cs"

# Then validator test (T008), then implementation (T009):
Task: "Write CreateRentalValidatorTests in tests/.../Unit/CreateRentalValidatorTests.cs"
Task: "Implement CreateRentalValidator in src/.../Validators/CreateRentalValidator.cs"
```

## Parallel Example: User Stories (after Phase 2)

```bash
# All user stories can start simultaneously after Phase 2:
Agent A: Phase 3 (US1 — Rent a Film)     — T011 → T012 → T013
Agent B: Phase 4 (US2 — List/Filter)      — T014 → T015 → T016
Agent C: Phase 5 (US3 — View Details)     — T017 → T018 → T019
Agent D: Phase 6 (US4 — Return Rental)    — T020 → T021 → T022
Agent E: Phase 7 (US5 — Delete Rental)    — T023 → T024 → T025
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Phase 1: Verify entity scaffolding
2. Complete Phase 2: Foundation (DTOs, validator + tests, service stub, controller shell, test helper)
3. Complete Phase 3: US1 — Rent a Film (TDD)
4. **STOP and VALIDATE**: US1 independently testable — staff can create rentals with inventory resolution
5. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 + 2 → Foundation ready
2. Add US1 → Test → Deploy (staff can create rentals with auto inventory resolution)
3. Add US2 → Test → Deploy (staff can browse/filter rental history)
4. Add US3 → Test → Deploy (staff can view full rental details with display names)
5. Add US4 → Test → Deploy (staff can process returns)
6. Add US5 → Test → Deploy (admins can clean up erroneous records)
7. Phase 8 → Polish → Final validation

---

## Notes

- [P] tasks = different files, no dependencies on each other
- [USn] label maps task to specific user story for traceability
- TDD is NON-NEGOTIABLE: write tests first (RED), implement (GREEN), refactor
- Validator tested with FluentValidation.TestHelper (TestValidate + ShouldHaveValidationErrorFor)
- Integration tests use Testcontainers.PostgreSql via shared TestWebAppFactory (IClassFixture)
- AutoFixture generates anonymous test data; Customize&lt;T&gt; for domain-specific defaults
- All service methods return Ardalis.Result&lt;T&gt; or Result — no exceptions for expected outcomes
- All validation errors MUST be aggregated (FluentValidation .AsErrors() + FK/business checks combined)
- DTOs follow constitution v1.9.0: lean list (IDs only), flat detail (inlined names), domain enum types
- Inventory resolution: query Inventory WHERE FilmId+StoreId AND no active rental, ORDER BY InventoryId ASC
- Distinguish "not stocked" vs "all copies rented" in create error messages (FR-009)
- Customer.Activebool and Staff.Active checked for create validation
- Payment.RentalId checked before hard delete (referential integrity)
- Return endpoint (PUT /return) has no request body — sets ReturnDate=UtcNow automatically
- Commit after each task or logical group completes
- Stop at any checkpoint to validate story independently
