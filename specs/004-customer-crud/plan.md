# Implementation Plan: Customer CRUD API

**Branch**: `004-customer-crud` | **Date**: 2026-02-22 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/004-customer-crud/spec.md`

## Summary

Implement a full RESTful CRUD API for the Customer entity (GET list with search/pagination, GET by ID, POST, PUT, DELETE as soft-delete) using controller-based routing, a service layer over the existing DvdrentalContext, FluentValidation for input, immutable record DTOs, and comprehensive TDD with Testcontainers.

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (LTS, patch 10.0.3)
**Primary Dependencies**: ASP.NET Core 10.0, EF Core 10.0 + Npgsql 10.0.0, FluentValidation 11.11, Swashbuckle 10.1.4
**Storage**: PostgreSQL 18 via Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0
**Testing**: xUnit 2.9.3, FluentAssertions 8.8.0, AutoFixture 4.18.1 + AutoFixture.Xunit2, Testcontainers.PostgreSql 4.10.0
**Target Platform**: Linux (WSL2 for development)
**Project Type**: web-service (RESTful API backend for SPA)
**Performance Goals**: < 1 second for paginated list/search with up to 10,000 customer records
**Constraints**: Controller-based routing ONLY (no minimal APIs), TDD mandatory, functional/immutable style preferred
**Scale/Scope**: dvdrental sample DB (~599 customers in seed data, up to 10K target)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Spec-Driven | PASS | spec.md complete, clarify phase done, plan in progress |
| II. Test-First | PASS | TDD red-green-refactor will be followed; every acceptance scenario maps to tests |
| III. Clean Architecture | PASS | Controller → ICustomerService → DvdrentalContext; dependencies point inward |
| IV. YAGNI | PASS | No repository pattern (service uses DbContext directly); no speculative abstractions |
| V. Observability | PASS | Structured logging for mutations; XML docs on public APIs; actionable error messages |
| VI. Functional Style | PASS | Record DTOs with init-only properties; pure mapping functions; side-effects isolated to service boundary |

**New NuGet packages (justification per Dependency Policy):**

| Package | Justification |
|---------|---------------|
| FluentValidation.AspNetCore 11.11.0 | Constitution requires input validation; FluentValidation provides declarative, testable validators that keep validation logic separate from controllers (Clean Architecture) |
| AutoFixture 4.18.1 | Constitution mandates AutoFixture for anonymous test data generation |
| AutoFixture.Xunit2 4.18.1 | Integrates AutoFixture with xUnit [AutoData] attribute for cleaner test signatures |

## Project Structure

### Documentation (this feature)

```text
specs/004-customer-crud/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── customers-api.md # REST endpoint contracts
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/RentalForge.Api/
├── Controllers/
│   ├── HealthController.cs          # Existing
│   └── CustomersController.cs       # NEW: Customer CRUD endpoints
├── Data/
│   ├── DvdrentalContext.cs          # Existing (no changes needed)
│   └── Entities/
│       └── Customer.cs              # Existing (no changes needed)
├── Models/
│   ├── HealthResponse.cs            # Existing
│   ├── CustomerResponse.cs          # NEW: GET response DTO
│   ├── CreateCustomerRequest.cs     # NEW: POST request DTO
│   ├── UpdateCustomerRequest.cs     # NEW: PUT request DTO
│   └── PagedResponse.cs             # NEW: Generic pagination wrapper
├── Services/
│   ├── ICustomerService.cs          # NEW: Service interface
│   └── CustomerService.cs           # NEW: Service implementation
├── Validators/
│   ├── CreateCustomerValidator.cs   # NEW: POST validation rules
│   └── UpdateCustomerValidator.cs   # NEW: PUT validation rules
└── Program.cs                       # MODIFIED: Register services + FluentValidation

tests/RentalForge.Api.Tests/
├── Infrastructure/
│   └── TestWebAppFactory.cs         # Existing (no changes needed)
├── Integration/
│   ├── HealthEndpointTests.cs       # Existing
│   └── CustomerEndpointTests.cs     # NEW: Integration tests via WebApplicationFactory
└── Unit/
    ├── CreateCustomerValidatorTests.cs  # NEW: Validator tests
    └── UpdateCustomerValidatorTests.cs  # NEW: Validator tests
```

**Structure Decision**: Extends the existing single-project structure. New directories `Services/` and `Validators/` follow Clean Architecture (controllers → services → DbContext) without introducing unnecessary project boundaries (YAGNI). The `Unit/` test directory is new — existing tests are all integration; unit tests for validators go here. Service behavior is verified through integration tests via WebApplicationFactory (YAGNI — no separate service unit tests needed).

## Complexity Tracking

> No constitution violations to justify. All design choices align with principles.

| Decision | Rationale | Simpler Alternative Rejected |
|----------|-----------|------------------------------|
| Service interface (ICustomerService) | Constitution III requires interfaces at layer boundaries; enables testability | Direct DbContext in controller — violates Clean Architecture |
| FluentValidation package | Constitution requires input validation; declarative validators are independently testable | Manual validation in controller — mixes concerns, harder to test |
| AutoFixture package | Constitution mandates AutoFixture for test data | Hand-coded test data — violates constitution |
