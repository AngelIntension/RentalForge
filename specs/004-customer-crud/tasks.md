# Tasks: Customer CRUD API

**Input**: Design documents from `/specs/004-customer-crud/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/customers-api.md

**Tests**: TDD is NON-NEGOTIABLE per constitution v1.5.0. Every task follows red-green-refactor. Failing tests are written before implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add NuGet packages required by this feature

- [X] T001 Add FluentValidation.AspNetCore 11.11.0 NuGet package to src/RentalForge.Api/RentalForge.Api.csproj
- [X] T002 [P] Add AutoFixture 4.18.1 and AutoFixture.Xunit2 4.18.1 NuGet packages to tests/RentalForge.Api.Tests/RentalForge.Api.Tests.csproj

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create DTOs, service interface, controller shell, validators (TDD), test data helper, and DI registration. All user stories depend on these artifacts.

**CRITICAL**: No user story work can begin until this phase is complete.

### DTOs (all records with init-only properties per Principle VI)

- [X] T003 [P] Create PagedResponse\<T\> generic record in src/RentalForge.Api/Models/PagedResponse.cs per data-model.md (Items, Page, PageSize, TotalCount, TotalPages)
- [X] T004 [P] Create CustomerResponse record in src/RentalForge.Api/Models/CustomerResponse.cs per data-model.md (Id, StoreId, FirstName, LastName, Email, AddressId, IsActive, CreateDate, LastUpdate)
- [X] T005 [P] Create CreateCustomerRequest record in src/RentalForge.Api/Models/CreateCustomerRequest.cs per data-model.md (FirstName, LastName, Email, StoreId, AddressId)
- [X] T006 [P] Create UpdateCustomerRequest record in src/RentalForge.Api/Models/UpdateCustomerRequest.cs per data-model.md (FirstName, LastName, Email, StoreId, AddressId)

### Service Layer (interface + skeleton)

- [X] T007 Create ICustomerService interface with 5 method signatures in src/RentalForge.Api/Services/ICustomerService.cs: GetCustomersAsync(search, page, pageSize), GetCustomerByIdAsync(id), CreateCustomerAsync(request), UpdateCustomerAsync(id, request), DeactivateCustomerAsync(id)
- [X] T008 Create CustomerService skeleton implementing ICustomerService (all methods throw NotImplementedException) with DvdrentalContext and ILogger\<CustomerService\> injected via primary constructor in src/RentalForge.Api/Services/CustomerService.cs

### Validators (TDD: write failing tests first, then implement)

- [X] T009 [P] TDD Red: Write CreateCustomerValidator unit tests using AutoFixture in tests/RentalForge.Api.Tests/Unit/CreateCustomerValidatorTests.cs — test all rules from data-model.md validation table: required FirstName/LastName, max lengths (45/45/50), email format when provided, StoreId/AddressId > 0
- [X] T010 [P] TDD Red: Write UpdateCustomerValidator unit tests using AutoFixture in tests/RentalForge.Api.Tests/Unit/UpdateCustomerValidatorTests.cs — same rules as CreateCustomerValidator
- [X] T011 [P] TDD Green: Implement CreateCustomerValidator with FluentValidation rules to pass T009 tests in src/RentalForge.Api/Validators/CreateCustomerValidator.cs
- [X] T012 [P] TDD Green: Implement UpdateCustomerValidator with FluentValidation rules to pass T010 tests in src/RentalForge.Api/Validators/UpdateCustomerValidator.cs

### Controller and DI

- [X] T013 Create CustomersController inheriting ControllerBase with [ApiController] [Route("api/customers")] and 5 endpoint stubs (GET list, GET by id, POST, PUT, DELETE) delegating to ICustomerService in src/RentalForge.Api/Controllers/CustomersController.cs — include XML docs and Swagger annotations per contracts/customers-api.md
- [X] T014 Register ICustomerService/CustomerService (scoped), FluentValidation auto-validation with AddFluentValidationAutoValidation(), and validators in src/RentalForge.Api/Program.cs

### Test Data Infrastructure

- [X] T015 Create CustomerTestHelper with static methods to seed test data (Address, Store, Staff, Customer) into DvdrentalContext for integration tests in tests/RentalForge.Api.Tests/Infrastructure/CustomerTestHelper.cs — handle Store↔Staff circular FK with SET session_replication_role, use CityId from reference data

### Verification

- [X] T016 Verify dotnet build succeeds and all validator unit tests pass with dotnet test --filter "FullyQualifiedName~Validator"

**Checkpoint**: Foundation ready — DTOs compiled, validators tested, controller shell responds (with NotImplementedException from service), test helper ready. User story implementation can now begin.

---

## Phase 3: User Story 1 — Browse and Search Customers (Priority: P1) MVP

**Goal**: Staff can list active customers with search (by name/email) and pagination, receiving results in PagedResponse format.

**Independent Test**: Send GET /api/customers with various search/pagination params and verify filtered, paginated results with correct metadata.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation (TDD Red)**

- [X] T017 [US1] TDD Red: Write integration tests for GET /api/customers in tests/RentalForge.Api.Tests/Integration/CustomerEndpointTests.cs — seed test data via CustomerTestHelper, test: (1) list returns paginated active customers with default page size, (2) search by partial first name returns filtered results (case-insensitive), (3) search by partial last name, (4) search by partial email, (5) page 2 with pageSize=10 returns correct page with metadata, (6) search with no matches returns empty list with zero totalCount, (7) pageSize > 100 capped to 100, (8) page < 1 returns 400 validation error

### Implementation for User Story 1

- [X] T018 [US1] TDD Green: Implement GetCustomersAsync in src/RentalForge.Api/Services/CustomerService.cs — filter Activebool==true, apply search ILIKE on FirstName/LastName/Email (OR logic), order by LastName then FirstName, offset pagination with Skip/Take, return PagedResponse\<CustomerResponse\> with totalCount via CountAsync
- [X] T019 [US1] Verify all US1 integration tests pass and no existing tests are broken with dotnet test

**Checkpoint**: Staff can browse and search customers. GET /api/customers returns paginated, searchable results. MVP deliverable.

---

## Phase 4: User Story 2 — View Customer Details (Priority: P1)

**Goal**: Staff can view full details of a specific active customer by ID.

**Independent Test**: Send GET /api/customers/{id} and verify correct CustomerResponse or 404 for missing/deactivated customers.

### Tests for User Story 2

- [X] T020 [US2] TDD Red: Write integration tests for GET /api/customers/{id} in tests/RentalForge.Api.Tests/Integration/CustomerEndpointTests.cs — test: (1) existing active customer returns 200 with full CustomerResponse, (2) non-existent ID returns 404, (3) deactivated customer returns 404

### Implementation for User Story 2

- [X] T021 [US2] TDD Green: Implement GetCustomerByIdAsync in src/RentalForge.Api/Services/CustomerService.cs — find by ID where Activebool==true, return CustomerResponse or null (controller returns 404)
- [X] T022 [US2] Verify all US1+US2 tests pass with dotnet test

**Checkpoint**: Staff can browse, search, and view individual customer details. Both read operations are complete.

---

## Phase 5: User Story 3 — Register a New Customer (Priority: P2)

**Goal**: Staff can create a new customer with validated input, receiving the new customer's details with a unique ID.

**Independent Test**: Send POST /api/customers with valid/invalid payloads and verify 201 with CustomerResponse, 400 for validation errors, or 400 for invalid FK references.

### Tests for User Story 3

- [X] T023 [US3] TDD Red: Write integration tests for POST /api/customers in tests/RentalForge.Api.Tests/Integration/CustomerEndpointTests.cs — test: (1) valid request returns 201 with CustomerResponse and Location header, (2) missing required fields returns 400 with field-specific errors, (3) invalid email format returns 400, (4) non-existent StoreId returns 400, (5) non-existent AddressId returns 400, (6) created customer appears in GET list

### Implementation for User Story 3

- [X] T024 [US3] TDD Green: Implement CreateCustomerAsync in src/RentalForge.Api/Services/CustomerService.cs — validate Store/Address exist via AnyAsync, create Customer entity (Activebool=true, Active=1, CreateDate=today, LastUpdate=now), SaveChangesAsync, return CustomerResponse
- [X] T025 [US3] Verify all US1-US3 tests pass with dotnet test

**Checkpoint**: Staff can browse, search, view, and register customers. Full read + create operations complete.

---

## Phase 6: User Story 4 — Update Customer Information (Priority: P2)

**Goal**: Staff can update an active customer's fields (full replacement) with validated input.

**Independent Test**: Send PUT /api/customers/{id} with valid/invalid payloads and verify 200 with updated CustomerResponse, 400 for validation errors, or 404 for missing/deactivated customers.

### Tests for User Story 4

- [X] T026 [US4] TDD Red: Write integration tests for PUT /api/customers/{id} in tests/RentalForge.Api.Tests/Integration/CustomerEndpointTests.cs — test: (1) valid update returns 200 with updated CustomerResponse and refreshed lastUpdate, (2) invalid data returns 400, (3) non-existent customer returns 404, (4) deactivated customer returns 404, (5) updated customer reflects changes in subsequent GET

### Implementation for User Story 4

- [X] T027 [US4] TDD Green: Implement UpdateCustomerAsync in src/RentalForge.Api/Services/CustomerService.cs — find active customer, validate Store/Address exist, update fields (full replacement), set LastUpdate=now, SaveChangesAsync, return CustomerResponse
- [X] T028 [US4] Verify all US1-US4 tests pass with dotnet test

**Checkpoint**: Full read/write CRUD minus delete. Staff can browse, search, view, register, and update customers.

---

## Phase 7: User Story 5 — Deactivate a Customer (Priority: P3)

**Goal**: Staff can soft-delete a customer by setting active status to false. Deactivated customers are excluded from all active queries.

**Independent Test**: Send DELETE /api/customers/{id} and verify 204 for success, 404 for missing/already-deactivated, and that deactivated customers no longer appear in GET list or GET by ID.

### Tests for User Story 5

- [X] T029 [US5] TDD Red: Write integration tests for DELETE /api/customers/{id} in tests/RentalForge.Api.Tests/Integration/CustomerEndpointTests.cs — test: (1) active customer returns 204, (2) already-deactivated customer returns 404, (3) non-existent customer returns 404, (4) deactivated customer excluded from GET list, (5) deactivated customer returns 404 on GET by ID

### Implementation for User Story 5

- [X] T030 [US5] TDD Green: Implement DeactivateCustomerAsync in src/RentalForge.Api/Services/CustomerService.cs — find active customer, set Activebool=false and Active=0, set LastUpdate=now, SaveChangesAsync, return true/false (controller returns 204 or 404)
- [X] T031 [US5] Verify all tests pass (full suite) with dotnet test

**Checkpoint**: All 5 CRUD operations complete. Full customer lifecycle: browse, search, view, register, update, deactivate.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Logging, documentation, and final validation across all user stories

- [X] T032 Add structured logging for customer mutations (create, update, deactivate) using ILogger in src/RentalForge.Api/Services/CustomerService.cs — log operation, customerId, and outcome at Information level per Principle V
- [X] T033 Verify Swagger UI documents all 5 customer endpoints correctly by writing a Swagger metadata integration test in tests/RentalForge.Api.Tests/Integration/CustomerEndpointTests.cs (verify paths, operation IDs, response types)
- [X] T034 Run full test suite (dotnet test), verify dotnet build clean, and validate quickstart.md curl scenarios work against running API

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3–7)**: All depend on Foundational phase completion
  - User stories SHOULD proceed sequentially in priority order (P1 → P2 → P3)
  - US1 and US2 (both P1) MAY run in parallel if desired
  - US3 and US4 (both P2) MAY run in parallel after US1/US2
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: After Foundational — no dependency on other stories
- **US2 (P1)**: After Foundational — no dependency on other stories (parallel with US1 possible)
- **US3 (P2)**: After Foundational — no dependency on US1/US2 but recommended after (test verifies created customer appears in list)
- **US4 (P2)**: After Foundational — no dependency on US3 but recommended after (test uses existing customer)
- **US5 (P3)**: After Foundational — tests verify deactivated customer excluded from GET list (recommends US1 complete)

### Within Each User Story (TDD Cycle)

1. Tests MUST be written and MUST fail before implementation (TDD Red)
2. Implement minimum code to pass tests (TDD Green)
3. Refactor if needed
4. Verify all prior tests still pass (regression)
5. Story complete before moving to next priority

### Parallel Opportunities

- T001 and T002 can run in parallel (different .csproj files)
- T003, T004, T005, T006 can all run in parallel (different model files)
- T009 and T010 can run in parallel (different test files)
- T011 and T012 can run in parallel (different validator files)
- US1 and US2 can run in parallel (different endpoints, different service methods)
- US3 and US4 can run in parallel (different endpoints, different service methods)

---

## Parallel Example: Phase 2 (Foundational)

```bash
# Launch all DTOs in parallel:
T003: "Create PagedResponse<T> in src/RentalForge.Api/Models/PagedResponse.cs"
T004: "Create CustomerResponse in src/RentalForge.Api/Models/CustomerResponse.cs"
T005: "Create CreateCustomerRequest in src/RentalForge.Api/Models/CreateCustomerRequest.cs"
T006: "Create UpdateCustomerRequest in src/RentalForge.Api/Models/UpdateCustomerRequest.cs"

# After DTOs, launch validator tests in parallel:
T009: "Write CreateCustomerValidator tests in tests/.../Unit/CreateCustomerValidatorTests.cs"
T010: "Write UpdateCustomerValidator tests in tests/.../Unit/UpdateCustomerValidatorTests.cs"

# After test files, launch validator implementations in parallel:
T011: "Implement CreateCustomerValidator in src/.../Validators/CreateCustomerValidator.cs"
T012: "Implement UpdateCustomerValidator in src/.../Validators/UpdateCustomerValidator.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (NuGet packages)
2. Complete Phase 2: Foundational (DTOs, service shell, validators, controller, DI)
3. Complete Phase 3: User Story 1 (Browse & Search)
4. **STOP and VALIDATE**: Test US1 independently — GET /api/customers returns paginated results
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Browse & Search) → Test independently → MVP!
3. Add US2 (View Details) → Test independently → Read operations complete
4. Add US3 (Register) → Test independently → Create operation added
5. Add US4 (Update) → Test independently → Update operation added
6. Add US5 (Deactivate) → Test independently → Full CRUD complete
7. Polish → Logging, Swagger verification, final validation

### TDD Discipline Per Story

Each story follows strict red-green-refactor:
1. Write failing integration tests (Red)
2. Implement service method to pass tests (Green)
3. Refactor if needed
4. Verify regression (all prior tests still pass)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Verify tests fail before implementing (TDD Red)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- AutoFixture MUST be used for anonymous test data in unit tests (constitution mandate)
- CustomerTestHelper handles Store↔Staff circular FK for integration test data seeding
