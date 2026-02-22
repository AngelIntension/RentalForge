# Research: Customer CRUD API

**Feature**: 004-customer-crud | **Date**: 2026-02-22

## R-001: Service Layer Pattern (Clean Architecture)

**Decision**: Use `ICustomerService` interface with `CustomerService` implementation that takes `DvdrentalContext` directly. No repository pattern.

**Rationale**: Constitution III requires interfaces at layer boundaries and inward-pointing dependencies. The controller depends on `ICustomerService` (abstraction), and the service depends on `DvdrentalContext` (EF Core is already an abstraction over the database). Adding a repository on top of EF Core's DbSet would be a redundant abstraction (violates YAGNI, Principle IV).

**Alternatives considered**:
- Repository pattern over DbContext: Rejected — adds indirection without value when EF Core already provides unit-of-work and queryable abstractions. The dvdrental schema is fixed (scaffolded), and we have no plans to swap ORMs.
- CQRS/MediatR: Rejected — over-engineering for a simple CRUD feature. No complex command/query separation needed.

## R-002: FluentValidation Integration

**Decision**: Use FluentValidation.AspNetCore 11.11.0 with automatic validation via the ASP.NET Core pipeline. Validators registered via `AddFluentValidationAutoValidation()`.

**Rationale**: FluentValidation provides declarative, testable validation rules that keep validation logic out of controllers (Clean Architecture). Each request DTO gets its own validator class. Failed validation returns 400 with structured error details matching ASP.NET Core's `ValidationProblemDetails` format.

**Alternatives considered**:
- Data Annotations on DTOs: Rejected — mixes validation concerns into model classes; harder to test independently; limited expressiveness for cross-field rules.
- Manual validation in controller: Rejected — violates separation of concerns; duplicates logic across endpoints.

## R-003: Soft Delete Implementation

**Decision**: Soft delete sets `Activebool = false` and `Active = 0` on the Customer entity. List/search/get-by-id endpoints filter by `Activebool == true`. No global query filter — filter explicitly in the service.

**Rationale**: The dvdrental schema has two active fields: `activebool` (boolean, primary) and `active` (integer, legacy). Updating both maintains consistency with existing seed data patterns. Explicit filtering (rather than EF Core global query filter) keeps the behavior transparent and avoids surprises when querying for admin/reporting purposes later.

**Alternatives considered**:
- EF Core global query filter (`HasQueryFilter`): Rejected — makes it invisible when inactive records are excluded, complicates future "show all customers" admin endpoints, and the existing data layer tests don't account for it.
- Hard delete: Rejected per spec — customer records must be preserved for historical rental/payment data integrity.

## R-004: Pagination Strategy

**Decision**: Offset-based pagination using `Skip((page - 1) * pageSize).Take(pageSize)` with a total count query. Return metadata in the response body as a wrapper object (`PagedResponse<T>`).

**Rationale**: Offset pagination is simple, stateless, and sufficient for the expected data volume (up to 10K customers). The response includes `totalCount`, `totalPages`, `page`, and `pageSize` so the client can render pagination controls.

**Alternatives considered**:
- Cursor-based pagination (keyset): Rejected — over-engineering for this scale; adds complexity without benefit at <10K records. Can be adopted later if needed.
- Link headers (RFC 5988): Rejected — less discoverable for SPA clients; body-embedded metadata is more practical.

## R-005: Search Implementation

**Decision**: Single `search` query parameter applies case-insensitive `ILIKE` (via EF Core `Contains` + PostgreSQL collation) across `FirstName`, `LastName`, and `Email` with OR logic. Translated to `WHERE first_name ILIKE '%term%' OR last_name ILIKE '%term%' OR email ILIKE '%term%'` by Npgsql.

**Rationale**: EF Core's `string.Contains()` translates to `ILIKE` on PostgreSQL (case-insensitive by default with Npgsql). A single search parameter is simpler for the client and matches the spec's OR-logic requirement. For 10K records, ILIKE with no index is acceptable performance.

**Alternatives considered**:
- Separate filters per field (`firstName`, `lastName`, `email`): Rejected — spec explicitly defines a single `search` parameter with OR logic.
- PostgreSQL full-text search (`tsvector`): Rejected — over-engineering; requires schema changes (new tsvector column on customer table) and is not needed for simple LIKE matching at this scale.

## R-006: AutoFixture for Test Data

**Decision**: Use AutoFixture 4.18.1 with AutoFixture.Xunit2 for `[AutoData]` attribute. Customize to generate valid Customer-like data (constrained string lengths, valid email formats) where AutoFixture defaults would create invalid data.

**Rationale**: Constitution mandates AutoFixture for anonymous test data. Using `[AutoData]` and `[InlineAutoData]` keeps tests focused on behavior rather than setup. Custom `ISpecimenBuilder` or `ICustomization` classes handle domain constraints (e.g., FirstName max 45 chars, valid email pattern).

**Alternatives considered**:
- Hand-coded test data only: Rejected — violates constitution mandate for AutoFixture.
- Bogus/Faker: Not needed — AutoFixture with customizations covers the requirement; adding another test data library is unnecessary.

## R-007: Error Response Format

**Decision**: Use ASP.NET Core's built-in `ValidationProblemDetails` for validation errors (400) and `ProblemDetails` for not-found (404). FluentValidation integrates with this automatically.

**Rationale**: `ValidationProblemDetails` is the standard ASP.NET Core format for validation errors — it includes `title`, `status`, `errors` (dictionary of field → messages). The constitution requires "consistent problem-details conventions for error payloads." Using the built-in format means no custom error DTOs needed.

**Alternatives considered**:
- Custom error envelope: Rejected — reinvents what ASP.NET Core already provides; adds maintenance burden.
- RFC 7807 custom implementation: Rejected — `ProblemDetails` already implements RFC 7807.

## R-008: DTO Design (Functional Style)

**Decision**: All DTOs are C# `record` types with `init`-only properties. Request DTOs use nullable reference types to distinguish "not provided" from "empty." Response DTOs are fully populated (no nulls for required fields).

**Rationale**: Constitution VI mandates immutable data structures. Records provide value equality, concise syntax, and immutability by default. Init-only properties allow JSON deserialization while preventing mutation after construction.

**Alternatives considered**:
- Mutable POCO DTOs: Rejected — violates Principle VI (Functional Style).
- `readonly record struct`: Rejected — unnecessary allocation optimization for DTOs in a web API; reference semantics (class records) are more natural here.
