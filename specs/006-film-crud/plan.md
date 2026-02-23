# Implementation Plan: Film CRUD API

**Branch**: `006-film-crud` | **Date**: 2026-02-23 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/006-film-crud/spec.md`

## Summary

Implement full RESTful CRUD and rich search/pagination for the Film entity,
following the established Customer CRUD patterns. The Film API adds complexity
via multi-table search (actors, categories), dedicated filters (category,
rating, year range), a detail endpoint with flat related data (actor names,
category names, language name), and hard delete with cascade on join tables
but inventory-blocking referential integrity. DTOs follow constitution v1.8.0:
lean list items (IDs only), rich flat detail, enum string/numeric converters.

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
**Performance Goals**: < 1 second for paginated list/search on 10,000 films
**Constraints**: Controller-based routing only, TDD red-green-refactor,
  Ardalis.Result for service error handling, aggregate all validation errors
**Scale/Scope**: ~1,000 films in dvdrental sample data, 5 endpoints

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Spec-Driven Development | PASS | spec.md approved with clarifications |
| II | Test-First (NON-NEGOTIABLE) | PASS | TDD planned for all production code |
| III | Clean Architecture | PASS | Controller → Service → DbContext layers |
| IV | YAGNI and Simplicity | PASS | Minimal scope per spec; no bulk/inventory |
| V | Observability & Maintainability | PASS | Structured logging, XML docs planned |
| VI | Functional Style & Immutability | PASS | Record DTOs, init-only, Result<T> returns |

**Technology Stack compliance:**
- Controller-based routing: PASS (FilmsController : ControllerBase)
- Result-based error handling: PASS (Result<T>/Result for all service methods)
- Aggregate validation: PASS (FluentValidation + FK checks aggregated)
- DTO structure v1.8.0: PASS (lean list, flat detail, enum converters)
- Testcontainers: PASS (integration tests against disposable PostgreSQL)
- AutoFixture: PASS (anonymous test data generation)

## Project Structure

### Documentation (this feature)

```text
specs/006-film-crud/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── films-api.md     # Endpoint contracts
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/RentalForge.Api/
├── Controllers/
│   └── FilmsController.cs          # Film CRUD endpoints
├── Services/
│   ├── IFilmService.cs             # Service interface
│   └── FilmService.cs              # Service implementation
├── Models/
│   ├── FilmListResponse.cs         # Lean list DTO (IDs, no names)
│   ├── FilmDetailResponse.cs       # Rich detail DTO (flat names)
│   ├── CreateFilmRequest.cs        # Create request DTO
│   └── UpdateFilmRequest.cs        # Update request DTO
├── Validators/
│   ├── CreateFilmValidator.cs      # FluentValidation for create
│   └── UpdateFilmValidator.cs      # FluentValidation for update
└── Data/
    └── Entities/                   # Existing: Film, FilmActor, etc.

tests/RentalForge.Api.Tests/
├── Integration/
│   └── FilmEndpointTests.cs        # Integration tests
├── Unit/
│   ├── CreateFilmValidatorTests.cs # Validator unit tests
│   └── UpdateFilmValidatorTests.cs # Validator unit tests
└── Infrastructure/
    ├── TestWebAppFactory.cs        # Existing (shared)
    └── FilmTestHelper.cs           # Film-specific seed data
```

**Structure Decision**: Follows the established monorepo layout from
004-customer-crud. New files added to existing directories — no new
projects or structural changes required.

## Complexity Tracking

> No violations. All design choices align with constitution principles.

| Consideration | Decision | Rationale |
|---------------|----------|-----------|
| Two response DTOs (list vs detail) | FilmListResponse + FilmDetailResponse | Constitution v1.8.0: lean list (IDs), rich detail (flat names) |
| Multi-table search (actors) | EF Core join + ILike in single query | YAGNI: no full-text search engine needed for ~1K films |
| Hard delete with cascade | Service checks inventory, then deletes | Cascade on film_actor/film_category via EF; inventory blocks |
