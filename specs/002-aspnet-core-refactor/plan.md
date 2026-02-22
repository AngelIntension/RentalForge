# Implementation Plan: ASP.NET Core Controller Refactor

**Branch**: `002-aspnet-core-refactor` | **Date**: 2026-02-21 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-aspnet-core-refactor/spec.md`

## Summary

Refactor the RentalForge API from minimal API endpoint extensions
to ASP.NET Core controller-based routing, as mandated by
constitution v1.3.0. The single existing `/health` endpoint
migrates from a `MapGet` extension method in `Endpoints/` to a
`ControllerBase`-derived class in `Controllers/`. All behavior,
response contracts, OpenAPI metadata, and integration tests MUST
remain functionally identical.

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (LTS, patch 10.0.3)
**Primary Dependencies**: ASP.NET Core 10.0, EF Core 10.0,
Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0,
Swashbuckle.AspNetCore 10.1.4, Microsoft.AspNetCore.OpenApi 10.0.3
**Storage**: PostgreSQL 18 (existing `dvdrental` sample database)
**Testing**: xUnit 2.9.3, FluentAssertions 8.8.0,
Testcontainers.PostgreSql 4.10.0,
Microsoft.AspNetCore.Mvc.Testing 10.0.3
**Target Platform**: Linux (WSL2 for development)
**Project Type**: Web service (REST API)
**Performance Goals**: Health endpoint responds within 2s (healthy),
5s (unhealthy) — existing test assertions
**Constraints**: Controller-based routing mandated by constitution
v1.3.0; minimal APIs prohibited for production endpoints
**Scale/Scope**: Single endpoint refactor; no new functionality

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1
design.*

| Principle | Gate | Status | Notes |
|-----------|------|--------|-------|
| I. Spec-Driven | Spec approved before implementation | PASS | spec.md exists and passed clarify |
| II. Test-First | Tests written before implementation | PASS | Existing tests cover all acceptance scenarios; they MUST continue passing. No new behavior = no new tests required. |
| III. Clean Architecture | Separation of concerns, dependency inversion | PASS | Controller handles HTTP concerns only; DB access is infrastructure, not business logic — acceptable for a health probe per YAGNI (IV). |
| IV. YAGNI | No speculative abstractions | PASS | No service layer added for the health check — a service class would be premature abstraction for a simple DB probe with no business logic. |
| V. Observability | Structured logging, XML docs | PASS | Existing logging preserved; XML docs added to new controller. |
| VI. Functional Style | Immutable data, pure functions | PASS | HealthResponse is already an immutable record. |
| Web Framework | Controllers, no minimal APIs | PASS | This feature implements the mandate. |

**YAGNI vs Clean Architecture note**: The health check performs
a direct database probe (`SELECT version()`, `SELECT NOW()`) to
report operational status. This is infrastructure/operational
logic, not business logic. Per Principle IV, extracting a service
class for this trivial operation would be premature abstraction.
If future health checks grow in complexity (multiple subsystems,
circuit breakers), a service extraction can be justified at that
time. Conflict resolution: III > IV, but the conflict does not
apply here because the controller contains no business logic.

## Project Structure

### Documentation (this feature)

```text
specs/002-aspnet-core-refactor/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── health.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/RentalForge.Api/
├── Controllers/
│   └── HealthController.cs    # NEW — replaces Endpoints/HealthEndpoint.cs
├── Models/
│   └── HealthResponse.cs      # MOVED from Endpoints/HealthEndpoint.cs
├── Data/
│   ├── DvdrentalContext.cs     # UNCHANGED
│   └── Entities/               # UNCHANGED (16 entity files)
├── Program.cs                  # MODIFIED — AddControllers/MapControllers
├── RentalForge.Api.csproj      # UNCHANGED (no new packages)
└── appsettings.json            # UNCHANGED

tests/RentalForge.Api.Tests/
├── Infrastructure/
│   └── TestWebAppFactory.cs    # UNCHANGED (WebApplicationFactory works with controllers)
└── Integration/
    ├── DataLayerTests.cs       # UNCHANGED
    └── HealthEndpointTests.cs  # UNCHANGED (tests HTTP behavior, not implementation)

REMOVED:
├── src/RentalForge.Api/Endpoints/              # DELETED (entire directory)
│   └── HealthEndpoint.cs                       # DELETED
```

**Structure Decision**: Existing two-project structure
(`src/RentalForge.Api` + `tests/RentalForge.Api.Tests`) is
retained. The only structural changes are: (1) new `Controllers/`
directory, (2) new `Models/` directory for response DTOs, (3)
removal of `Endpoints/` directory. No new projects needed.

## Complexity Tracking

> No violations. All changes are minimal and directly required by
> the spec and constitution mandate.
