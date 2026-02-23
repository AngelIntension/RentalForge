# Implementation Plan: Result Pattern Refactor

**Branch**: `005-result-pattern-refactor` | **Date**: 2026-02-22 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/005-result-pattern-refactor/spec.md`

## Summary

Replace the exception-based validation flow (`ServiceValidationException`) in the
Customer CRUD API with the Ardalis.Result pattern. Service methods will return
`Result<T>` / `Result` instead of throwing exceptions for validation failures and
not-found conditions. FluentValidation will move from ASP.NET pipeline auto-validation
into the service layer so all validation errors (input + FK existence) can be aggregated
into a single `Result<T>.Invalid()` response. Controllers will use explicit
`result.Status` switch expressions to translate results to HTTP responses. The legacy
exception class and all try/catch blocks will be removed.

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (LTS, patch 10.0.3)
**Primary Dependencies**:
- Ardalis.Result 10.1.0 (core Result types)
- Ardalis.Result.AspNetCore 10.1.0 (HTTP translation utilities)
- Ardalis.Result.FluentValidation 10.1.0 (`.AsErrors()` bridge)
- FluentValidation.AspNetCore 11.3.1 (existing — kept for DI registration)
- ASP.NET Core 10.0 / EF Core 10.0 / Npgsql 10.0.0 (existing)

**Storage**: PostgreSQL 18 (dvdrental database, unchanged)
**Testing**: xUnit 2.9.3 + FluentAssertions 8.8.0 + Testcontainers.PostgreSql 4.10.0 + AutoFixture 4.18.1 (all existing, unchanged)
**Target Platform**: Linux (WSL2 for development)
**Project Type**: Web service (ASP.NET Core API)
**Performance Goals**: N/A (behavior-preserving refactoring, no performance changes)
**Constraints**: All 90 existing tests MUST pass with zero modifications to test assertions
**Scale/Scope**: 5 service methods, 1 controller (5 actions), 1 exception class to remove

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-Driven Development | PASS | Spec approved, plan in progress |
| II. Test-First (NON-NEGOTIABLE) | PASS | This is a behavior-preserving refactoring — all 90 existing tests serve as the specification. No new behavior means no new tests required before implementation. Tests MUST pass after each change. |
| III. Clean Architecture | PASS | Service layer returns Result types; controllers handle HTTP translation only; no layer violations |
| IV. YAGNI and Simplicity | PASS | No speculative abstractions — manual switch expressions over global convention registration; 3 NuGet packages justified by constitution mandate (see Complexity Tracking) |
| V. Observability and Maintainability | PASS | Existing logging preserved; no new log points needed |
| VI. Functional Style and Immutability | PASS | Result types replace exceptions for expected outcomes per principle; enables composable error handling |

**Post-Design Re-check**: PASS — All principles satisfied. The Ardalis.Result
packages are mandated by the constitution v1.7.0 Technology Stack section.

## Project Structure

### Documentation (this feature)

```text
specs/005-result-pattern-refactor/
├── plan.md              # This file
├── research.md          # Phase 0 output — library research
├── spec.md              # Feature specification
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (files modified by this refactoring)

```text
src/RentalForge.Api/
├── RentalForge.Api.csproj            # Add 3 Ardalis.Result packages
├── Program.cs                        # Remove AddFluentValidationAutoValidation()
├── Controllers/
│   └── CustomersController.cs        # Replace try/catch with result.Status switch
├── Services/
│   ├── ICustomerService.cs           # Update return types to Result<T>/Result
│   ├── CustomerService.cs            # Return Result types; inject validators
│   └── ServiceValidationException.cs # DELETE
└── Validators/                       # Unchanged (validators injected into service)
    ├── CreateCustomerValidator.cs
    └── UpdateCustomerValidator.cs

tests/RentalForge.Api.Tests/          # Unchanged — all 90 tests pass as-is
CLAUDE.md                             # Update Active Technologies + Key Constraints
```

**Structure Decision**: No new files or directories. This is a refactoring of
existing files within the established project structure. One file is deleted
(`ServiceValidationException.cs`).

## Design Decisions

### D1: Manual Controller Translation vs `[TranslateResultToActionResult]`

**Decision**: Use explicit `result.Status` switch expressions in the controller.

**Rationale**:
- `CreatedAtAction()` generates route-based Location headers; the
  `[TranslateResultToActionResult]` attribute uses `Created(uri, value)` instead,
  which would require the service layer to know URL paths (violates Clean Architecture).
- The attribute's `NotFound` mapping returns `ProblemDetails` in the body; our
  existing tests expect a bare 404 with no body. Manual mapping preserves this.
- Only 5 actions in one controller — switch expressions are simple and readable.

**Alternative rejected**: `[TranslateResultToActionResult]` — leaks HTTP routing
into service layer for Created, changes 404 response body shape.

### D2: Move FluentValidation Into Service Layer

**Decision**: Inject `IValidator<T>` into `CustomerService` and call validators
explicitly. Remove `AddFluentValidationAutoValidation()` from `Program.cs`.

**Rationale**:
- FR-002 requires aggregation of input validation + FK existence errors in a
  single failure result. This is impossible when FluentValidation runs at the
  ASP.NET pipeline level (it short-circuits before the service is called).
- FR-007 and the constitution mandate using the `.AsErrors()` bridge from
  `Ardalis.Result.FluentValidation`.
- `AddFluentValidationAutoValidation()` is deprecated in newer FluentValidation
  versions; manual validator invocation is the recommended pattern.
- `AddValidatorsFromAssemblyContaining<>()` is kept for DI registration
  (comes from `FluentValidation.DependencyInjectionExtensions`, included in
  `FluentValidation.AspNetCore`).

**Impact on existing tests**: All pass. The response format (`ValidationProblemDetails`)
is identical whether errors come from the ASP.NET pipeline or from the controller
translating `Result.Invalid`. Error field identifiers match because `.AsErrors()`
uses `ValidationFailure.PropertyName` as `ValidationError.Identifier`.

### D3: `GetCustomersAsync` Return Type Unchanged

**Decision**: `GetCustomersAsync` continues to return
`Task<PagedResponse<CustomerResponse>>` (no Result wrapper).

**Rationale**: This method cannot fail for expected reasons — it always returns a
(possibly empty) paginated result. Per FR-001, Result types are for "validation
failures, not-found conditions, and business-rule violations." Wrapping a
never-failing method adds noise without value.

### D4: Result Status Mapping

| Service Return | Controller Translation | HTTP Response |
|----------------|----------------------|---------------|
| `Result<T>.Created(value)` | `CreatedAtAction(nameof(GetCustomer), ...)` | 201 + Location |
| `Result<T>.Success(value)` | `Ok(result.Value)` | 200 |
| `Result.NoContent()` | `NoContent()` | 204 |
| `Result<T>.NotFound()` | `NotFound()` | 404 (bare, no body) |
| `Result<T>.Invalid(errors)` | `ValidationProblem(ModelState)` | 400 + ValidationProblemDetails |

### D5: Validation Error Identifier Casing

FK existence errors use camelCase identifiers (`"storeId"`, `"addressId"`) to
match the current behavior. FluentValidation uses PascalCase (`"FirstName"`,
`"StoreId"`) from C# property names. This matches the current behavior where
auto-validation produces PascalCase keys and `ServiceValidationException` uses
camelCase keys. No changes needed.

## Implementation Sequence

### Phase 1: Setup (NuGet packages)

1. Add `Ardalis.Result`, `Ardalis.Result.AspNetCore`,
   `Ardalis.Result.FluentValidation` (all 10.1.0) to `RentalForge.Api.csproj`.
2. Verify `dotnet build` succeeds.

### Phase 2: Service Layer (US1)

1. Update `ICustomerService` — change return types for `GetCustomerByIdAsync`,
   `CreateCustomerAsync`, `UpdateCustomerAsync`, `DeactivateCustomerAsync`.
2. Update `CustomerService` constructor — inject `IValidator<CreateCustomerRequest>`
   and `IValidator<UpdateCustomerRequest>`.
3. Refactor `CreateCustomerAsync`:
   - Call `validator.ValidateAsync(request)` → if invalid, collect via `.AsErrors()`.
   - Check FK existence → collect `ValidationError` instances.
   - Aggregate all errors → return `Result<CustomerResponse>.Invalid(allErrors)`.
   - On success → return `Result<CustomerResponse>.Created(response)`.
4. Refactor `UpdateCustomerAsync`:
   - Lookup customer → not found → return `Result<CustomerResponse>.NotFound()`.
   - Call `validator.ValidateAsync(request)` → collect errors via `.AsErrors()`.
   - Check FK existence → collect `ValidationError` instances.
   - Aggregate all errors → return `Result<CustomerResponse>.Invalid(allErrors)`.
   - On success → return `Result<CustomerResponse>.Success(response)`.
5. Refactor `GetCustomerByIdAsync`:
   - Return `Result<CustomerResponse>.NotFound()` instead of `null`.
   - Return `Result<CustomerResponse>.Success(response)` on success.
6. Refactor `DeactivateCustomerAsync`:
   - Return `Result.NotFound()` instead of `false`.
   - Return `Result.NoContent()` instead of `true`.

### Phase 3: Controller Layer (US2)

1. Update `CustomersController` — remove `ServiceValidationException` using
   directive and all try/catch blocks.
2. Add a private `InvalidResult` helper that converts
   `IEnumerable<ValidationError>` to `ValidationProblem(ModelState)`.
3. Refactor each action to use `result.Status` switch expression:
   - `GetCustomer`: Ok → `Ok(result.Value)`, NotFound → `NotFound()`.
   - `CreateCustomer`: Created → `CreatedAtAction(...)`, Invalid → `InvalidResult(...)`.
   - `UpdateCustomer`: Ok → `Ok(result.Value)`, NotFound → `NotFound()`,
     Invalid → `InvalidResult(...)`.
   - `DeactivateCustomer`: NoContent → `NoContent()`, NotFound → `NotFound()`.
4. Remove `AddFluentValidationAutoValidation()` from `Program.cs`.

### Phase 4: Cleanup (US3)

1. Delete `ServiceValidationException.cs`.
2. Verify zero references to `ServiceValidationException` in codebase.
3. Run full test suite — all 90 tests MUST pass.

### Phase 5: Documentation (US4)

1. Update `CLAUDE.md`:
   - Add Ardalis.Result 10.1.0, Ardalis.Result.AspNetCore 10.1.0,
     Ardalis.Result.FluentValidation 10.1.0 to Active Technologies.
   - Update Key Constraints: service methods return Result types for expected
     outcomes; exceptions reserved for unexpected failures.
   - Update Recent Changes section.

## Complexity Tracking

| Item | Why Needed | Simpler Alternative Rejected Because |
|------|------------|-------------------------------------|
| 3 NuGet packages (Ardalis.Result ecosystem) | Constitution v1.7.0 mandates Ardalis.Result with companion packages | Custom Result type would violate YAGNI and re-invent validated library |
| Moving FluentValidation into service | FR-002 requires aggregating input + FK errors in single response | Keeping auto-validation makes aggregation impossible (pipeline short-circuits) |
