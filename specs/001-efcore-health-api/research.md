# Research: EF Core Scaffold and Health API

**Feature Branch**: `001-efcore-health-api`
**Date**: 2026-02-21

## .NET Runtime and Language Version

**Decision**: .NET 10.0 (LTS) with C# 14 and EF Core 10.0

**Rationale**: .NET 10 was released 2025-11-11 as an LTS release
(supported until 2028-11-10). Latest patch is 10.0.3 (2026-02-10).
The constitution mandates "latest stable LTS version" — .NET 10
is that version. EF Core 10.0 ships alongside it.

**Alternatives considered**:
- .NET 8 (previous LTS): Still supported until 2026-11, but .NET 10
  is newer, LTS, and production-ready. No reason to use the older LTS.
- .NET 9 (STS): Support ends May 2026. Not LTS. Rejected per
  constitution.

## EF Core Scaffolding Approach

**Decision**: Scaffold with `dotnet ef dbcontext scaffold` then
transition to code-first migrations for future changes.

**Rationale**: The dvdrental database already exists with 15 tables.
Scaffolding generates accurate entity classes and `DbContext`
configuration from the live schema, avoiding manual transcription
errors. Post-scaffold, code-first migrations give version-controlled
schema evolution.

**Key considerations**:
- Scaffolded entities will use mutable classes by default. Per
  constitution Principle VI, these MUST be reviewed and converted
  to records where appropriate (especially DTOs / value objects).
- EF Core requires mutable entity classes for change tracking —
  this is a justified framework constraint per Principle VI.
- The `--data-annotations` flag is NOT used; Fluent API configuration
  in `OnModelCreating` is preferred to keep entities clean.
- Custom types in dvdrental (`mpaa_rating` ENUM, `year` domain type,
  `tsvector`, `text[]`) require Npgsql-specific mappings.

**Alternatives considered**:
- Hand-write all entities: Error-prone for 15 tables with complex
  relationships. Rejected.
- Database-first only (no migrations): Locks out future schema
  evolution via code. Rejected.

## dvdrental Schema Summary

**Decision**: Target all 15 tables in the dvdrental schema.

15 tables with relationships:

| Table | PK | Notable FK relationships |
|-------|-----|--------------------------|
| actor | actor_id | — |
| address | address_id | city_id → city |
| category | category_id | — |
| city | city_id | country_id → country |
| country | country_id | — |
| customer | customer_id | store_id → store, address_id → address |
| film | film_id | language_id → language, original_language_id → language |
| film_actor | (actor_id, film_id) | actor_id → actor, film_id → film |
| film_category | (film_id, category_id) | film_id → film, category_id → category |
| inventory | inventory_id | film_id → film, store_id → store |
| language | language_id | — |
| payment | payment_id | customer_id → customer, staff_id → staff, rental_id → rental |
| rental | rental_id | inventory_id → inventory, customer_id → customer, staff_id → staff |
| staff | staff_id | address_id → address, store_id → store |
| store | store_id | manager_staff_id → staff, address_id → address |

**Notable**: staff ↔ store is a circular reference (staff.store_id
→ store, store.manager_staff_id → staff). EF Core scaffolding
handles this automatically.

**Custom PostgreSQL types**:
- `mpaa_rating`: ENUM ('G', 'PG', 'PG-13', 'R', 'NC-17')
- `year`: Domain type over integer
- `tsvector`: Full-text search vector (film.fulltext)
- `text[]`: PostgreSQL array (film.special_features)

## Integration Testing with Testcontainers

**Decision**: Use `Testcontainers.PostgreSql` 4.10.0 with
`WebApplicationFactory<Program>` and `IAsyncLifetime`.

**Rationale**: Testcontainers provisions a real PostgreSQL container
per test run, ensuring tests exercise actual database behavior.
The `WebApplicationFactory` pattern allows overriding the connection
string to point at the container.

**Pattern chosen**: Collection Fixture (one container shared across
test classes). For this feature's single test, `IClassFixture` is
sufficient, but the Collection Fixture pattern is forward-compatible
with future test classes.

**Key packages**:
- `Testcontainers.PostgreSql` 4.10.0
- `Microsoft.AspNetCore.Mvc.Testing` (for `WebApplicationFactory`)
- `FluentAssertions` (for assertion readability)

**WSL2 considerations**:
- Docker must be running inside WSL2 (Docker Desktop with WSL2
  backend or native Docker Engine).
- The `/mnt/` filesystem has I/O overhead; tests still work but
  may be slightly slower than native Linux paths.
- Ryuk (resource reaper) container auto-cleans orphaned containers.

**Alternatives considered**:
- In-memory database: Does not support PostgreSQL-specific features
  (ENUM, tsvector, arrays). Rejected.
- Per-test container: Too slow for future expansion. Rejected.
- Respawn for cleanup: Not needed for this feature (single read-only
  health check test). Can be added later when write tests exist.

## Health Endpoint Design

**Decision**: Minimal API GET `/health` returning JSON with database
version, server time, and status.

**Rationale**: A minimal endpoint that executes `SELECT version()`
and `SELECT NOW()` against PostgreSQL. This is schema-independent,
so it works even if the dvdrental tables are absent.

**Response shape** (200 OK):
```json
{
  "status": "healthy",
  "databaseVersion": "PostgreSQL 18.x ...",
  "serverTime": "2026-02-21T12:00:00Z"
}
```

**Error response** (503 Service Unavailable):
```json
{
  "status": "unhealthy",
  "error": "Database connection failed: <message>"
}
```

**Alternatives considered**:
- ASP.NET Core built-in health checks (`Microsoft.Extensions.
  Diagnostics.HealthChecks`): Adds framework overhead and
  opinionated response format. For this simple feature, a direct
  endpoint is simpler (Principle IV: YAGNI). Can migrate later.
- Returning plain text: Rejected per FR-005 (structured JSON).

## OpenAPI / Swagger

**Decision**: Use Swashbuckle or the built-in .NET OpenAPI support
(available in .NET 10) to expose Swagger UI in development.

**Rationale**: Constitution requires all endpoints to have OpenAPI
metadata. .NET 10 has built-in OpenAPI document generation via
`Microsoft.AspNetCore.OpenApi`. Swagger UI via
`Swashbuckle.AspNetCore` or `Scalar.AspNetCore` provides a
browsable interface.

## NuGet Dependency Justification

| Package | Justification |
|---------|---------------|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Constitution-mandated ORM + provider |
| `Microsoft.EntityFrameworkCore.Design` | Required for `dotnet ef` scaffold/migrations tooling |
| `Microsoft.AspNetCore.OpenApi` | Constitution-mandated API documentation |
| `Swashbuckle.AspNetCore` | Swagger UI for development (constitution requires it) |
| `xunit` | Constitution-mandated test framework |
| `FluentAssertions` | Constitution-preferred assertion library |
| `Microsoft.AspNetCore.Mvc.Testing` | WebApplicationFactory for integration tests |
| `Testcontainers.PostgreSql` | Constitution-mandated DB test infrastructure |
| `Microsoft.NET.Test.Sdk` | Required xUnit test runner infrastructure |
