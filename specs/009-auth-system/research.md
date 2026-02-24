# Research: Authentication System

**Feature Branch**: `009-auth-system`
**Date**: 2026-02-24

## Decision 1: DbContext Strategy

**Decision**: Single DbContext — `DvdrentalContext` inherits from `IdentityDbContext<ApplicationUser, IdentityRole, string>`. Identity tables are isolated in a dedicated `identity` PostgreSQL schema.

**Rationale**: Using a single context avoids complications with cross-entity transactions (User-to-Customer FK), migrations, and DI. The `identity` schema prevents table-name collisions with the existing `dvdrental` public-schema tables. The context is already hand-managed (not scaffolded), so changing the base class is straightforward.

**Alternatives considered**:
- Two separate DbContexts (one for dvdrental, one for Identity): Rejected — complicates the User→Customer FK relationship, requires manual cross-context coordination for transactions, and doubles migration management overhead.
- Single context with default public schema: Rejected — risks naming collisions with existing `dvdrental` tables and makes migration diffs harder to read.

## Decision 2: JWT Package Choice

**Decision**: Use `Microsoft.IdentityModel.JsonWebTokens` 8.16.0 with `JsonWebTokenHandler` for token creation. `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.3 for middleware validation.

**Rationale**: `JsonWebTokenHandler` is the modern replacement for the legacy `JwtSecurityTokenHandler`. It is 30% faster, AOT-compatible, and is the default handler in ASP.NET Core 8+. Since we target .NET 10, this is the natural choice.

**Alternatives considered**:
- `System.IdentityModel.Tokens.Jwt` (legacy `JwtSecurityTokenHandler`): Rejected — flagged as legacy since IdentityModel 7.x. Same version cadence (8.16.0) but not recommended for new code.

## Decision 3: Rate Limiting

**Decision**: Use built-in ASP.NET Core rate limiting middleware (`builder.Services.AddRateLimiter()` / `app.UseRateLimiter()`). Fixed-window rate limiter per endpoint group.

**Rationale**: Built into the ASP.NET Core shared framework since .NET 7 — no additional NuGet package needed. Supports fixed window, sliding window, token bucket, and concurrency algorithms. Fixed window is the simplest and matches the spec's "N requests per minute" thresholds.

**Alternatives considered**:
- Third-party libraries (AspNetCoreRateLimit): Rejected — unnecessary given the built-in solution.
- Token bucket: Rejected — more complex than needed for simple per-minute thresholds.

## Decision 4: Refresh Token Format

**Decision**: Opaque refresh tokens (cryptographically random base64 strings), not JWT-format. Family ID stored in the database alongside the token.

**Rationale**: Refresh tokens do not need to be self-contained (they are always validated server-side against the database). Opaque tokens are shorter, don't leak user information, and cannot be decoded by the client. The family ID is stored in the DB row for rotation/reuse detection.

**Alternatives considered**:
- JWT-format refresh tokens with embedded family claim: Rejected — unnecessarily complex, leaks claims to the client, and still requires DB lookup for rotation/revocation checks.

## Decision 5: Frontend Token Storage

**Decision**: Store both access token and refresh token in `localStorage`. Access token also held in memory for the current session.

**Rationale**: Per the user's explicit requirement ("localStorage + refresh token strategy per constitution"). `localStorage` persists across page reloads and browser restarts (FR-019). While `httpOnly` cookies would be more XSS-resistant, the user explicitly ruled them out.

**Alternatives considered**:
- httpOnly cookies: Rejected — explicitly excluded by the user.
- sessionStorage: Rejected — does not persist across browser restarts (violates FR-019).
- In-memory only: Rejected — does not persist across page reloads.

## Decision 6: Identity Tables Schema Layout

**Decision**: All Identity tables (`users`, `roles`, `user_roles`, `user_claims`, `user_logins`, `user_tokens`, `role_claims`) and the custom `refresh_tokens` table go in the `identity` PostgreSQL schema.

**Rationale**: Isolates auth concerns from the existing `dvdrental` tables in the `public` schema. Makes migrations cleaner and avoids any naming conflicts. Consistent with the clean architecture principle of separation of concerns.

**Alternatives considered**:
- Default `public` schema with `AspNet` prefix: Rejected — clutters the schema, still risks confusion with existing tables.

## Decision 7: New NuGet Packages

| Package | Version | Justification |
|---------|---------|---------------|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.3 | ASP.NET Core Identity with EF Core store — required for user/role management per constitution |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.3 | JWT bearer token validation middleware — required per constitution |

**Note**: `Microsoft.IdentityModel.JsonWebTokens` and `Microsoft.IdentityModel.Tokens` are transitive dependencies of JwtBearer — no explicit package reference needed. Rate limiting is built into the ASP.NET Core framework — no package needed.

## Decision 8: Frontend Auth Architecture

**Decision**: `AuthProvider` context wrapping the router. `useAuth` hook exposes login/logout/refresh/user/role. API client reads token from a shared getter (not directly from localStorage) and auto-refreshes before expiry.

**Rationale**: Centralizes auth state in React context, consistent with the existing provider-stack pattern (`ThemeProvider > QueryClientProvider > AuthProvider > RouterProvider`). The API client's token accessor pattern allows the refresh logic to be encapsulated in one place.

**Alternatives considered**:
- Zustand/Redux store: Rejected — YAGNI. React context is sufficient for auth state.
- Token refresh via interceptor library (axios interceptors): Rejected — project uses native `fetch` via custom `api` client. Adding axios would be unnecessary.

## Decision 9: ApplicationUser Custom Properties

**Decision**: `ApplicationUser` extends `IdentityUser` with a nullable `CustomerId` (int?) FK to the `dvdrental` `Customer` entity. No other custom properties beyond what Identity provides.

**Rationale**: Per clarification — the Customer link enables per-record data scoping for Customer-role users. The FK is nullable because Admin and Staff users don't have a corresponding Customer record, and newly registered Customers may not yet be linked.

**Alternatives considered**:
- Email-based matching (no FK): Rejected — fragile, requires email alignment, no referential integrity.
- Separate linking table: Rejected — YAGNI. A simple nullable FK is sufficient for a 1:0..1 relationship.
