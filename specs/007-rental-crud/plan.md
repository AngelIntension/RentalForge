# Implementation Plan: Rental CRUD API

**Branch**: `007-rental-crud` | **Date**: 2026-02-23 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/007-rental-crud/spec.md`

## Summary

Implement full RESTful CRUD for the Rental entity, following the established
Customer and Film CRUD patterns. The Rental API introduces unique complexity
via inventory resolution — the create endpoint accepts a filmId + storeId and
the service layer resolves to one available inventory copy (no active rental).
A dedicated return sub-resource endpoint (PUT /return) processes rental returns.
Hard delete is protected by payment referential integrity. DTOs follow
constitution v1.9.0: lean list items (IDs only), rich flat detail (customer
name, film title, staff name), domain enum types on DTO properties.

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (LTS, patch 10.0.3)
**Primary Dependencies**: ASP.NET Core 10.0, EF Core 10.0 + Npgsql 10.0.0,
  Ardalis.Result 10.1.0, FluentValidation 11.3.1, Swashbuckle 10.1.4
**Storage**: PostgreSQL 18 (`dvdrental` sample database) via Testcontainers
  for tests, `dotnet user-secrets` for dev connection string
**Testing**: xUnit 2.9.3, FluentAssertions 8.8.0, AutoFixture 4.18.1,
  Microsoft.AspNetCore.Mvc.Testing 10.0.3, Testcontainers.PostgreSql 4.10.0
**Target Platform**: Linux server (WSL2 for development)
**Project Type**: Web service (ASP.NET Core RESTful API)
**Performance Goals**: < 1 second for paginated list/filter on 50,000 rentals
**Constraints**: Controller-based routing only, TDD red-green-refactor,
  Ardalis.Result for service error handling, aggregate all validation errors,
  DTO enum properties use domain enum types (constitution v1.9.0)
**Scale/Scope**: ~16,000 rentals in dvdrental sample data, 5 endpoints

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Spec-Driven Development | PASS | spec.md approved, clarify confirmed no ambiguities |
| II | Test-First (NON-NEGOTIABLE) | PASS | TDD planned for all production code |
| III | Clean Architecture | PASS | Controller → Service → DbContext layers |
| IV | YAGNI and Simplicity | PASS | Minimal scope: no payments, no bulk, no inventory mgmt |
| V | Observability & Maintainability | PASS | Structured logging, XML docs planned |
| VI | Functional Style & Immutability | PASS | Record DTOs, init-only, Result<T> returns |

**Technology Stack compliance:**
- Controller-based routing: PASS (RentalsController : ControllerBase)
- Result-based error handling: PASS (Result<T>/Result for all service methods)
- Aggregate validation: PASS (FluentValidation + FK/business checks aggregated)
- DTO structure v1.9.0: PASS (lean list, flat detail, domain enum types)
- Testcontainers: PASS (integration tests against disposable PostgreSQL)
- AutoFixture: PASS (anonymous test data generation)

## Project Structure

### Documentation (this feature)

```text
specs/007-rental-crud/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── rentals-api.md   # Endpoint contracts
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/RentalForge.Api/
├── Controllers/
│   └── RentalsController.cs          # Rental CRUD + return endpoints
├── Services/
│   ├── IRentalService.cs             # Service interface
│   └── RentalService.cs              # Service implementation (inventory resolution)
├── Models/
│   ├── RentalListResponse.cs         # Lean list DTO (IDs, no names)
│   ├── RentalDetailResponse.cs       # Rich detail DTO (flat names)
│   └── CreateRentalRequest.cs        # Create request DTO
├── Validators/
│   └── CreateRentalValidator.cs      # FluentValidation for create
└── Data/
    └── Entities/                     # Existing: Rental, Inventory, etc.

tests/RentalForge.Api.Tests/
├── Integration/
│   └── RentalEndpointTests.cs        # Integration tests
├── Unit/
│   └── CreateRentalValidatorTests.cs # Validator unit tests
└── Infrastructure/
    ├── TestWebAppFactory.cs          # Existing (shared)
    └── RentalTestHelper.cs           # Rental-specific seed data
```

**Structure Decision**: Follows the established monorepo layout from
004-customer-crud and 006-film-crud. New files added to existing directories —
no new projects or structural changes required.

## Complexity Tracking

> No violations. All design choices align with constitution principles.

| Consideration | Decision | Rationale |
|---------------|----------|-----------|
| Inventory resolution in service | Service queries Inventory + Rental to find available copy | Spec FR-008: filmId+storeId resolves to inventory transparently |
| Two response DTOs (list vs detail) | RentalListResponse + RentalDetailResponse | Constitution v1.9.0: lean list (IDs), rich detail (flat names) |
| Return as sub-resource endpoint | PUT /api/rentals/{id}/return | Semantically distinct from full update; spec-driven design |
| Hard delete with payment protection | Service checks payments, then deletes | Protects referential integrity; consistent with film-inventory pattern |
