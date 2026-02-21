# Implementation Plan: EF Core Scaffold and Health API

**Branch**: `001-efcore-health-api` | **Date**: 2026-02-21 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-efcore-health-api/spec.md`

## Summary

Scaffold the existing dvdrental PostgreSQL database into an EF Core
data access layer, expose a `/health` Minimal API endpoint that
returns database version and server time, configure secrets
management via `dotnet user-secrets`, enable Swagger/OpenAPI, and
include one xUnit integration test using Testcontainers for
PostgreSQL.

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (LTS, patch 10.0.3)
**Primary Dependencies**: EF Core 10.0, Npgsql.EntityFrameworkCore.PostgreSQL, Swashbuckle.AspNetCore, Microsoft.AspNetCore.OpenApi
**Storage**: PostgreSQL 18 (existing `dvdrental` sample database at localhost:5432)
**Testing**: xUnit + FluentAssertions + Testcontainers.PostgreSql 4.10.0 + Microsoft.AspNetCore.Mvc.Testing
**Target Platform**: Linux (WSL2 for development)
**Project Type**: web-service (ASP.NET Core Minimal API)
**Performance Goals**: /health response < 2s (healthy), < 5s (unhealthy timeout)
**Constraints**: Single /health endpoint; no authentication; integration test suite < 60s total
**Scale/Scope**: 15 scaffolded entities, 1 API endpoint, 5 integration tests (3 health + 2 data layer)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status |
|-----------|------|--------|
| I. Spec-Driven | Approved spec exists before implementation | PASS — spec.md complete, clarify found no ambiguities |
| II. Test-First | Tests written before implementation; acceptance scenarios mapped | PASS — US1 has 3 integration tests, US2 has 2, US3 validates test infrastructure; TDD cycle followed per-story (US2 scaffold TDD exception documented in Complexity Tracking) |
| III. Clean Architecture | Layers separated; domain does not depend on infrastructure | PASS — Data/ layer (EF Core) is infrastructure; Endpoints/ is presentation; health logic is minimal and inline (justified by Principle IV) |
| IV. YAGNI | No speculative features; all code justified by spec | PASS — Single endpoint, scaffolded entities only, no repository pattern, no extra abstractions |
| V. Observability | Structured logging; XML docs; actionable errors | PASS — 503 response includes error detail; endpoint documented via OpenAPI; XML docs required on public types |
| VI. Functional Style | Immutable DTOs; side-effects confined | PASS — HealthResponse is a record; DB access confined to endpoint handler; EF entities remain mutable (justified framework constraint) |
| Constitution: Secrets | Connection strings via user-secrets | PASS — appsettings.json has empty placeholder; actual value in user-secrets |
| Constitution: Testing | Testcontainers for DB tests | PASS — Integration test uses disposable PostgreSql container |
| Constitution: OpenAPI | All endpoints in Swagger | PASS — /health has OpenAPI metadata; Swagger UI enabled in dev |

**No violations. No Complexity Tracking entries needed.**

## Project Structure

### Documentation (this feature)

```text
specs/001-efcore-health-api/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── health-endpoint.md  # /health API contract
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
RentalForge.sln

src/
└── RentalForge.Api/
    ├── RentalForge.Api.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── Data/
    │   ├── DvdrentalContext.cs
    │   └── Entities/
    │       ├── Actor.cs
    │       ├── Address.cs
    │       ├── Category.cs
    │       ├── City.cs
    │       ├── Country.cs
    │       ├── Customer.cs
    │       ├── Film.cs
    │       ├── FilmActor.cs
    │       ├── FilmCategory.cs
    │       ├── Inventory.cs
    │       ├── Language.cs
    │       ├── Payment.cs
    │       ├── Rental.cs
    │       ├── Staff.cs
    │       └── Store.cs
    └── Endpoints/
        └── HealthEndpoint.cs

tests/
└── RentalForge.Api.Tests/
    ├── RentalForge.Api.Tests.csproj
    ├── Infrastructure/
    │   └── TestWebAppFactory.cs
    └── Integration/
        ├── HealthEndpointTests.cs
        └── DataLayerTests.cs
```

**Structure Decision**: Single-project layout. This is a Minimal
API with a scaffolded data layer — no frontend, no separate domain
library. The `src/` and `tests/` split with a single .sln at the
root follows standard .NET conventions. The `Data/` folder contains
all EF Core infrastructure (DbContext + entities). The `Endpoints/`
folder contains Minimal API endpoint definitions.

## Complexity Tracking

> Two justified framework constraints documented below. Neither violates a principle — both are pragmatic trade-offs with documented rationale.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| EF entities remain mutable classes | EF Core change tracking requires mutable properties | Records with init-only setters break EF Core's change tracker; this is a documented framework constraint |
| US2 scaffold tests written before scaffold output exists | TDD requires tests first, but scaffold generates the code under test | Hand-writing 15 entities to make tests pass first would violate YAGNI and introduce transcription errors; scaffold IS the implementation step |
