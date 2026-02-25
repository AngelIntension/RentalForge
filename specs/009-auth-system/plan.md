# Implementation Plan: Authentication System

**Branch**: `009-auth-system` | **Date**: 2026-02-24 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/009-auth-system/spec.md`

## Summary

Add a full authentication and authorization system to RentalForge. Backend uses ASP.NET Core Identity with JWT bearer tokens, single-use refresh token rotation with family invalidation, and built-in rate limiting. Frontend adds React auth context, protected routes, role-based UI rendering, and automatic token refresh. Three roles (Admin, Staff, Customer) with per-endpoint authorization. All existing endpoints updated with `[Authorize]` attributes. Full TDD on both sides.

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (backend), TypeScript 5.9 strict (frontend)
**Primary Dependencies**: ASP.NET Core Identity 10.0.3, JWT Bearer 10.0.3, Ardalis.Result 10.1.0, FluentValidation 11.3.1, React 19.2, React Router 7.13, TanStack Query 5.90, Zod 4.3
**Storage**: PostgreSQL 18 (existing `dvdrental` database + new `identity` schema)
**Testing**: xUnit 2.9.3 + FluentAssertions 8.8 + Testcontainers.PostgreSql 4.10 + AutoFixture 4.18.1 (backend), Vitest 4.0 + React Testing Library 16 + MSW 2.12 (frontend)
**Target Platform**: Linux (WSL2), browsers (SPA)
**Project Type**: Web application (monorepo: API backend + React SPA frontend)
**Performance Goals**: Login/register < 1s server-side, token refresh transparent to user
**Constraints**: JWT signing key via user-secrets only, Identity tables in `identity` schema, all auth endpoints rate-limited
**Scale/Scope**: Learning project — 3 roles, 5 auth endpoints, ~15 modified files backend, ~10 new/modified files frontend

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Spec-Driven | PASS | Spec approved, clarifications resolved, plan follows spec |
| II. Test-First | PASS | TDD planned for all new code — tests before implementation |
| III. Clean Architecture | PASS | Service layer (IAuthService) handles business logic, controller handles HTTP, frontend separated by API boundary |
| IV. YAGNI | PASS | No password reset, no email confirmation, no social login, no 2FA. Only what spec requires. |
| V. Observability | PASS | Structured logging via ILogger on auth operations. Error messages are actionable. |
| VI. Functional Style | PASS | Service returns Result<T>/Result. DTOs are records. RefreshToken uses init-only where possible. |
| Auth & Authorization | PASS | Identity + JWT per constitution. Three roles. [Authorize] on all endpoints. [AllowAnonymous] explicit. PBKDF2 hashing. Rate limiting on sensitive endpoints. |
| DTO Structure | PASS | Flat DTOs with IDs. UserDto returns customerId (int?) not nested Customer. |
| Error Aggregation | PASS | All validation errors aggregated before responding, per existing pattern. |
| Secrets Management | PASS | JWT key, issuer, audience via dotnet user-secrets. Not in committed files. |
| Dependency Policy | PASS | Two new NuGet packages justified in research.md. Rate limiting uses built-in framework. |

**Post-Phase 1 re-check**: All gates still pass. The `identity` schema isolation, single DbContext inheritance, and Result-based auth service align with all constitution principles.

## Project Structure

### Documentation (this feature)

```text
specs/009-auth-system/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── checklists/
│   └── requirements.md
├── contracts/
│   └── auth-api.md
└── tasks.md             # Created by /speckit.tasks
```

### Source Code (repository root)

```text
src/RentalForge.Api/
├── Controllers/
│   ├── AuthController.cs              # NEW — auth endpoints
│   ├── CustomersController.cs         # MODIFIED — add [Authorize]; GET {id} allows Customer (own record only), all other actions Staff/Admin
│   ├── FilmsController.cs             # MODIFIED — add [Authorize] with role policies
│   ├── RentalsController.cs           # MODIFIED — add [Authorize] with role policies
│   └── HealthController.cs            # MODIFIED — add [AllowAnonymous]
├── Data/
│   ├── DvdrentalContext.cs            # MODIFIED — inherit IdentityDbContext, identity schema config
│   ├── Entities/
│   │   ├── ApplicationUser.cs         # NEW — extends IdentityUser
│   │   └── RefreshToken.cs            # NEW — refresh token entity
│   ├── Migrations/
│   │   └── *_AddIdentitySchema.cs     # NEW — EF Core migration
│   └── Seeding/
│       └── DevDataSeeder.cs           # MODIFIED — seed default auth users
├── Models/
│   └── Auth/
│       ├── RegisterRequest.cs         # NEW
│       ├── LoginRequest.cs            # NEW
│       ├── RefreshRequest.cs          # NEW
│       ├── LogoutRequest.cs           # NEW
│       ├── AuthResponse.cs            # NEW
│       ├── RefreshResponse.cs         # NEW
│       └── UserDto.cs                 # NEW
├── Services/
│   ├── IAuthService.cs               # NEW — auth service interface
│   └── AuthService.cs                # NEW — auth service implementation
├── Validators/
│   ├── RegisterRequestValidator.cs    # NEW
│   ├── LoginRequestValidator.cs       # NEW
│   └── RefreshRequestValidator.cs     # NEW
└── Program.cs                         # MODIFIED — Identity, JWT, rate limiting setup

tests/RentalForge.Api.Tests/
├── Unit/
│   ├── RegisterRequestValidatorTests.cs   # NEW
│   ├── LoginRequestValidatorTests.cs      # NEW
│   └── RefreshRequestValidatorTests.cs    # NEW
├── Integration/
│   ├── AuthEndpointTests.cs               # NEW
│   ├── AuthorizationTests.cs              # NEW
│   └── RateLimitTests.cs                  # NEW
└── Infrastructure/
    ├── TestWebAppFactory.cs               # MODIFIED — Identity setup in test container
    └── AuthTestHelper.cs                  # NEW — helper for creating test users/tokens

src/RentalForge.Web/
├── src/
│   ├── app/
│   │   ├── providers.tsx              # MODIFIED — add AuthProvider
│   │   └── routes.tsx                 # MODIFIED — add auth routes, ProtectedRoute wrapping
│   ├── components/
│   │   ├── auth/
│   │   │   └── protected-route.tsx    # NEW — route guard component
│   │   └── layout/
│   │       ├── bottom-nav.tsx         # MODIFIED — role-based nav items
│   │       └── sidebar-nav.tsx        # MODIFIED — role-based nav items
│   ├── hooks/
│   │   └── use-auth.tsx               # NEW — AuthContext + useAuth hook
│   ├── lib/
│   │   ├── api-client.ts             # MODIFIED — add auth header, refresh interceptor
│   │   └── validators.ts             # MODIFIED — add login/register schemas
│   ├── pages/
│   │   ├── login.tsx                  # NEW
│   │   ├── register.tsx               # NEW
│   │   └── profile.tsx                # MODIFIED — real user data
│   ├── test/
│   │   └── mocks/
│   │       └── handlers.ts           # MODIFIED — add auth mock handlers
│   └── types/
│       └── auth.ts                    # NEW — auth type definitions
```

**Structure Decision**: Follows the existing monorepo layout. Backend changes are within `src/RentalForge.Api/` and `tests/RentalForge.Api.Tests/`. Frontend changes within `src/RentalForge.Web/src/`. No new projects — auth is added to the existing API project via Identity inheritance.

## Complexity Tracking

No constitution violations to justify. All design choices follow existing patterns:
- Single DbContext (no new project boundary)
- Service layer with Result<T> (established pattern)
- Controller-based routing (constitution requirement)
- Built-in rate limiting (no additional dependencies)
