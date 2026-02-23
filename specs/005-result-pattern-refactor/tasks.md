# Tasks: Result Pattern Refactor

**Input**: Design documents from `specs/005-result-pattern-refactor/`
**Prerequisites**: plan.md, spec.md, research.md, quickstart.md

**Tests**: No new tests required. This is a behavior-preserving refactoring —
all 90 existing tests serve as the specification and MUST pass unchanged after
each phase checkpoint.

**Organization**: Tasks grouped by user story (US1–US4) from spec.md.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Include exact file paths in descriptions

## Phase 1: Setup

**Purpose**: Add Ardalis.Result NuGet packages to the API project

- [x] T001 Add Ardalis.Result 10.1.0, Ardalis.Result.AspNetCore 10.1.0, and Ardalis.Result.FluentValidation 10.1.0 package references to src/RentalForge.Api/RentalForge.Api.csproj
- [x] T002 Verify build succeeds with `dotnet build`

**Checkpoint**: Packages installed, build green. No behavioral changes yet.

---

## Phase 2: User Story 1 — Service Layer Returns Typed Results (Priority: P1)

**Goal**: Refactor ICustomerService and CustomerService to return `Result<T>` /
`Result` instead of throwing `ServiceValidationException` or returning
null/bool. Inject FluentValidation validators into the service and use
`.AsErrors()` bridge to aggregate input + FK validation errors.

**Independent Test**: Call each service method and verify that validation
failures, not-found conditions, and successful operations all return the
correct Result status without exceptions being thrown.

**⚠️ NOTE**: The build will NOT compile until Phase 3 (US2) updates the
controller to consume the new Result return types. This is expected — US1
and US2 form an atomic compilable unit.

- [x] T003 [US1] Update ICustomerService interface return types in src/RentalForge.Api/Services/ICustomerService.cs — change `GetCustomerByIdAsync` to `Task<Result<CustomerResponse>>`, `CreateCustomerAsync` to `Task<Result<CustomerResponse>>`, `UpdateCustomerAsync` to `Task<Result<CustomerResponse>>`, `DeactivateCustomerAsync` to `Task<Result>`. Add `using Ardalis.Result;`. Leave `GetCustomersAsync` unchanged (cannot fail for expected reasons).
- [x] T004 [US1] Refactor CustomerService.GetCustomerByIdAsync in src/RentalForge.Api/Services/CustomerService.cs — return `Result<CustomerResponse>.NotFound()` instead of null when customer not found or inactive, return `Result<CustomerResponse>.Success(response)` on success
- [x] T005 [US1] Refactor CustomerService.CreateCustomerAsync in src/RentalForge.Api/Services/CustomerService.cs — inject `IValidator<CreateCustomerRequest>` into constructor, call `validator.ValidateAsync(request)` and use `.AsErrors()` to convert FluentValidation failures to `List<ValidationError>`, check FK existence for store/address and add `new ValidationError("storeId", ...)` and `new ValidationError("addressId", ...)` to the same error list, return `Result<CustomerResponse>.Invalid(allErrors)` if any errors exist, return `Result<CustomerResponse>.Created(response)` on success. Remove the `throw new ServiceValidationException(errors)` call.
- [x] T006 [US1] Refactor CustomerService.UpdateCustomerAsync in src/RentalForge.Api/Services/CustomerService.cs — inject `IValidator<UpdateCustomerRequest>` into constructor, return `Result<CustomerResponse>.NotFound()` when customer not found or inactive (replacing `return null`), call `validator.ValidateAsync(request)` and use `.AsErrors()`, aggregate with FK `ValidationError` instances, return `Result<CustomerResponse>.Invalid(allErrors)` if errors, return `Result<CustomerResponse>.Success(response)` on success. Remove the `throw new ServiceValidationException(errors)` call.
- [x] T007 [US1] Refactor CustomerService.DeactivateCustomerAsync in src/RentalForge.Api/Services/CustomerService.cs — return `Result.NotFound()` instead of `false` when customer not found or inactive, return `Result.NoContent()` instead of `true` on success

**Checkpoint**: Service layer fully migrated to Result pattern. Build does NOT
compile yet (controller expects old return types). Proceed immediately to US2.

---

## Phase 3: User Story 2 — Controller Translates Results to HTTP Responses (Priority: P2)

**Goal**: Update CustomersController to consume Result types from the service
and translate them to HTTP responses using explicit `result.Status` switch
expressions. Remove all try/catch blocks for ServiceValidationException.
Remove FluentValidation auto-validation from Program.cs.

**Independent Test**: Run the full integration test suite (`dotnet test`) — all
90 tests must pass with zero modifications.

- [x] T008 [US2] Add private `InvalidResult` helper method to CustomersController in src/RentalForge.Api/Controllers/CustomersController.cs — accepts `IEnumerable<ValidationError>`, iterates and calls `ModelState.AddModelError(error.Identifier, error.ErrorMessage)` for each, returns `ValidationProblem(ModelState)`. Add `using Ardalis.Result;`.
- [x] T009 [US2] Refactor CustomersController.GetCustomer action in src/RentalForge.Api/Controllers/CustomersController.cs — call `GetCustomerByIdAsync`, switch on `result.Status`: `ResultStatus.Ok` → `Ok(result.Value)`, `ResultStatus.NotFound` → `NotFound()`
- [x] T010 [US2] Refactor CustomersController.CreateCustomer action in src/RentalForge.Api/Controllers/CustomersController.cs — remove try/catch block, call `CreateCustomerAsync`, switch on `result.Status`: `ResultStatus.Created` → `CreatedAtAction(nameof(GetCustomer), new { id = result.Value.Id }, result.Value)`, `ResultStatus.Invalid` → `InvalidResult(result.ValidationErrors)`
- [x] T011 [US2] Refactor CustomersController.UpdateCustomer action in src/RentalForge.Api/Controllers/CustomersController.cs — remove try/catch block, call `UpdateCustomerAsync`, switch on `result.Status`: `ResultStatus.Ok` → `Ok(result.Value)`, `ResultStatus.NotFound` → `NotFound()`, `ResultStatus.Invalid` → `InvalidResult(result.ValidationErrors)`
- [x] T012 [US2] Refactor CustomersController.DeactivateCustomer action in src/RentalForge.Api/Controllers/CustomersController.cs — call `DeactivateCustomerAsync`, switch on `result.Status`: `ResultStatus.NoContent` → `NoContent()`, `ResultStatus.NotFound` → `NotFound()`
- [x] T013 [US2] Remove `builder.Services.AddFluentValidationAutoValidation();` from src/RentalForge.Api/Program.cs — keep `AddValidatorsFromAssemblyContaining<CreateCustomerValidator>()` for DI registration. Remove the `using FluentValidation.AspNetCore;` import if no longer needed.
- [x] T014 [US2] Run full test suite with `dotnet test` — all 93 tests pass

**Checkpoint**: Build compiles, all 90 tests pass. Service + controller fully
migrated to Result pattern. ServiceValidationException still exists but has
zero references.

---

## Phase 4: User Story 3 — Remove Legacy Exception Infrastructure (Priority: P3)

**Goal**: Delete `ServiceValidationException` and verify no references remain.

- [x] T015 [US3] Delete src/RentalForge.Api/Services/ServiceValidationException.cs
- [x] T016 [US3] Verify zero references to `ServiceValidationException` in codebase with `grep -r "ServiceValidationException" src/ tests/`
- [x] T017 [US3] Run full test suite with `dotnet test` — all 93 tests pass

**Checkpoint**: Legacy exception infrastructure completely removed. Codebase has
a single, consistent error-handling pattern (Result types).

---

## Phase 5: User Story 4 — Update Project Documentation (Priority: P4)

**Goal**: Update CLAUDE.md to reflect the Result pattern as the standard
error-handling approach.

- [x] T018 [US4] Update Active Technologies > Backend section in CLAUDE.md — add `Ardalis.Result 10.1.0 + Ardalis.Result.AspNetCore 10.1.0 + Ardalis.Result.FluentValidation 10.1.0` line
- [x] T019 [US4] Update Key Constraints section in CLAUDE.md — add constraint that service methods MUST return Result types for expected outcomes (validation failures, not-found, business-rule violations); exceptions reserved for unexpected failures
- [x] T020 [US4] Update Recent Changes section in CLAUDE.md — add 005-result-pattern-refactor summary noting Ardalis.Result migration, FluentValidation moved into service layer, ServiceValidationException removed

**Checkpoint**: Documentation reflects current conventions. New developers will
follow the Result pattern from day one.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification across all user stories

- [x] T021 Run quickstart.md verification checklist in specs/005-result-pattern-refactor/quickstart.md
- [x] T022 Final build and test verification with `dotnet build && dotnet test`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **US1 (Phase 2)**: Depends on Setup (Phase 1) — packages must be installed
- **US2 (Phase 3)**: Depends on US1 (Phase 2) — controller consumes Result types from service
- **US3 (Phase 4)**: Depends on US2 (Phase 3) — can only delete exception after all references removed
- **US4 (Phase 5)**: Depends on Setup (Phase 1) — can run in parallel with US3
- **Polish (Phase 6)**: Depends on all previous phases

### User Story Dependencies

```
Phase 1 (Setup)
    │
    ▼
Phase 2 (US1: Service Layer)
    │
    ▼
Phase 3 (US2: Controller) ──────► Phase 5 (US4: Documentation) [P]
    │
    ▼
Phase 4 (US3: Cleanup)
    │
    ▼
Phase 6 (Polish)
```

- **US1 → US2**: Sequential — controller must consume new Result return types
- **US2 → US3**: Sequential — exception class can only be deleted after all references removed
- **US4**: Can run in parallel with US3 (documentation doesn't depend on cleanup)
- **US1 + US2**: Form an atomic compilable unit — build does NOT compile between them

### Within Each User Story

- Interface changes before implementation
- Service methods refactored sequentially (same file)
- Controller methods refactored sequentially (same file)
- Test suite run at phase completion (not per-task)

### Parallel Opportunities

- **T018–T020 (US4)** can run in parallel with T015–T017 (US3) — different files
- Within US1: T004–T007 are sequential (same file: CustomerService.cs)
- Within US2: T008–T012 are sequential (same file: CustomersController.cs)

---

## Implementation Strategy

### Atomic Refactoring (US1 + US2 Together)

1. Complete Phase 1: Setup (add packages)
2. Complete Phase 2: US1 (service layer) — build breaks temporarily
3. Complete Phase 3: US2 (controller) — build restores, all 90 tests pass
4. **STOP and VALIDATE**: `dotnet test` — all 90 tests must pass
5. Complete Phase 4: US3 (delete exception class)
6. Complete Phase 5: US4 (update CLAUDE.md) — can overlap with US3
7. Complete Phase 6: Polish (final verification)

### Key Risk Mitigation

- **Build breakage between US1 and US2**: Expected and documented. Do not
  commit between Phase 2 and Phase 3 completion.
- **Test regression**: Run `dotnet test` after Phase 3, Phase 4, and Phase 6.
  Any test failure means the refactoring changed behavior — investigate
  immediately.
- **Validation error format**: The `InvalidResult` helper uses `ModelState`
  exactly like the current `ServiceValidationException` handler. Response
  body shape is identical (`ValidationProblemDetails`).

---

## Notes

- No new tests are written — the 90 existing tests ARE the specification
- No [P] markers within US1 or US2 — all tasks modify the same 1–2 files
- `GetCustomersAsync` return type is intentionally unchanged (cannot fail)
- Commit after each phase checkpoint, not after each task
- FluentValidation `PropertyName` produces PascalCase identifiers (e.g., "FirstName"); FK errors use camelCase (e.g., "storeId") — this matches existing behavior
