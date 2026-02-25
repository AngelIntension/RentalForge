# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RentalForge is a full-stack SPA rental management application for learning spec-driven AI development in Linux using GitHub spec-kit and Claude Code. The backend is a C# ASP.NET Core RESTful API and the frontend is a React/TypeScript Single Page Application, both in a monorepo.

## Architecture

- **Backend**: C# ASP.NET Core RESTful API with controller-based routing (no minimal APIs) per constitution v1.8.0
- **Frontend**: React 19 SPA with TypeScript (strict mode), Vite, Tailwind CSS, Shadcn UI
- **Auth**: ASP.NET Core Identity + JWT bearer tokens; roles: Admin, Staff, Customer
- **Database**: PostgreSQL 18 (`dvdrental` sample database at localhost:5432)
- **ORM**: EF Core 10.0 with Npgsql provider
- **Repo structure**: Monorepo — backend and frontend coexist with independent build pipelines

## Solution Structure

```
RentalForge.slnx                          # XML-based solution file
src/RentalForge.Api/                      # ASP.NET Core Web API
  Controllers/                            # API controllers (Auth, Customers, Films, Rentals, Health)
  Services/                               # Business logic (IAuthService, ICustomerService, etc.)
  Validators/                             # FluentValidation request validators
  Data/
    DvdrentalContext.cs                    # EF Core DbContext (IdentityDbContext)
    Entities/                             # EF Core entity classes (incl. ApplicationUser, RefreshToken)
    Migrations/                           # EF Core migrations
    ReferenceData/                        # HasData() seed configurations
    Seeding/                              # Dev data seeder + JSON seed files
  Models/                                 # DTOs / view models (incl. Auth/)
  Program.cs                              # App entry point
src/RentalForge.Web/                      # React/TypeScript SPA
  src/
    app/                                  # Root layout, routes, providers
    components/                           # UI components (auth, customers, films, rentals, layout, shared, ui)
    hooks/                                # Custom hooks (useAuth, useCustomers, useFilms, useRentals, useTheme)
    lib/                                  # API client, query client, validators, utils
    pages/                                # Route pages (login, register, profile, home, CRUD pages)
    types/                                # TypeScript type definitions
    test/                                 # Test setup, fixtures, MSW mocks
tests/RentalForge.Api.Tests/              # xUnit test project (unit + integration)
specs/                                    # Feature specifications (spec-kit)
postman/                                  # Postman collections & environments
```

## Build & Run Commands

### Backend

```bash
dotnet build                                           # build the solution
dotnet test                                            # run all backend tests (~206 tests)
dotnet test --filter "FullyQualifiedName~TestName"     # run a single test
dotnet run --project src/RentalForge.Api               # run the API (http://localhost:5089)
dotnet run --project src/RentalForge.Api -- --seed     # run with dev data seeding
```

### Frontend

```bash
cd src/RentalForge.Web
npm install                                            # install dependencies
npm run dev                                            # start Vite dev server (http://localhost:5173)
npm run build                                          # TypeScript compile + Vite production build
npm run test                                           # run all frontend tests (~91 tests, Vitest)
npm run test:watch                                     # run tests in watch mode
npm run test:coverage                                  # run tests with coverage report
npm run typecheck                                      # TypeScript type checking only
npm run lint                                           # ESLint
```

## Active Technologies

### Backend
- C# 14 / .NET 10.0 (LTS, patch 10.0.3)
- ASP.NET Core 10.0
- ASP.NET Core Identity + JWT bearer authentication
- EF Core 10.0 + Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0
- FluentValidation.AspNetCore 11.3.1
- Ardalis.Result 10.1.0 + Ardalis.Result.AspNetCore 10.1.0 + Ardalis.Result.FluentValidation 10.1.0
- Swashbuckle.AspNetCore 10.1.4 + Microsoft.AspNetCore.OpenApi 10.0.3

### Backend Testing
- xUnit 2.9.3 + FluentAssertions 8.8.0
- AutoFixture 4.18.1 + AutoFixture.Xunit2 4.18.1
- Microsoft.AspNetCore.Mvc.Testing 10.0.3 (WebApplicationFactory)
- Testcontainers.PostgreSql 4.10.0

### Frontend
- React 19.2 + TypeScript 5.9 (strict mode)
- Vite 7.3 (bundler/dev server with HMR)
- React Router 7.13
- TanStack React Query 5.90
- Zod 4.3 (runtime schema validation)
- Tailwind CSS 4.2 + Radix UI 1.4 + Shadcn UI (new-york style)
- Lucide React (icons) + Sonner (toast notifications)
- Vite PWA Plugin (progressive web app support)

### Frontend Testing
- Vitest 4.0 (test runner, jsdom environment)
- React Testing Library 16.3 + @testing-library/user-event 14.6
- MSW 2.12 (Mock Service Worker for HTTP mocking)

## API Endpoints

| Resource   | Endpoints |
|-----------|-----------|
| Health    | `GET /health` |
| Auth      | `POST /api/auth/register`, `/login`, `/refresh`, `/logout` |
| Customers | `GET /api/customers` (list/search/paginate), `GET/PUT/DELETE /api/customers/{id}`, `POST /api/customers` |
| Films     | `GET /api/films` (list/search/filter/paginate), `GET/PUT/DELETE /api/films/{id}`, `POST /api/films` |
| Rentals   | `GET /api/rentals` (list/paginate), `GET/PUT/DELETE /api/rentals/{id}`, `POST /api/rentals` |

## Key Constraints

- All API endpoints MUST use controller-based routing (no minimal APIs) per constitution v1.8.0
- Validation MUST aggregate all errors before responding — never early-return on first failure (constitution v1.8.0)
- TDD is NON-NEGOTIABLE — red-green-refactor for all production code
- AutoFixture MUST be used for anonymous test data generation
- Connection strings and secrets via `dotnet user-secrets` only — never committed
- Functional style with immutable data structures preferred (records, init-only properties)
- Service methods MUST return `Result<T>` / `Result` (Ardalis.Result) for expected outcomes (validation failures, not-found, business-rule violations); exceptions reserved for unexpected failures only
- DTOs MUST be flat and simple: return IDs for related entities (not embedded objects); inline related data as flat properties for one-level relationships; use nested structures only for multi-level relationships (constitution v1.8.0)
- Enum serialization MUST be configured globally via JSON serializer options (not per-property); converter MUST accept both numeric and string values (constitution v1.8.1)
- DTO enum properties MUST use the domain enum type (e.g., `MpaaRating?`), not `string`; manual `.ToString()` conversion is prohibited (constitution v1.9.0)
- Frontend: Zod v4 with `zod/v4` import path; `.check()` uses `ctx.value`/`ctx.issues` pattern
- Frontend: MSW URL matching in jsdom MUST use regex patterns (`/\/api\/films/`) not string paths — jsdom constructs full URLs that don't match string-path handlers
- Frontend: All localStorage access must be in try-catch (broken in Vitest jsdom environment)

## Completed Features

- **001-efcore-health-api**: EF Core setup + health endpoint
- **002-aspnet-core-refactor**: ASP.NET Core project restructure
- **003-db-schema-seeding**: DB schema seeding via HasData() + DevDataSeeder CLI
- **004-customer-crud**: Full Customer CRUD API with FluentValidation, service layer, TDD
- **005-result-pattern-refactor**: Exception-based → Ardalis.Result pattern migration
- **006-film-crud**: Full Film CRUD with multi-table search, category/rating/year filters, MpaaRating enum
- **007-rental-crud**: Full Rental CRUD API
- **008-react-frontend-scaffold**: React/TypeScript SPA with Vite, Tailwind, Shadcn UI, routing, data hooks
- **009-auth-system**: JWT auth system — Identity + JWT + refresh tokens, React AuthProvider, protected routes, rate limiting
