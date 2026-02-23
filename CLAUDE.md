# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RentalForge is a full-stack SPA rental management application for learning spec-driven AI development in Linux using GitHub spec-kit and Claude Code. The backend is a C# ASP.NET Core RESTful API and the frontend is a React/TypeScript Single Page Application, both in a monorepo.

## Architecture

- **Backend**: C# ASP.NET Core RESTful API with controller-based routing (no minimal APIs) per constitution v1.8.0
- **Frontend**: React SPA with TypeScript (strict mode) — not yet scaffolded
- **Auth**: ASP.NET Core Identity + JWT bearer tokens; roles: Admin, Staff, Customer
- **Database**: PostgreSQL 18 (`dvdrental` sample database at localhost:5432)
- **ORM**: EF Core 10.0 with Npgsql provider
- **Repo structure**: Monorepo — backend and frontend coexist with independent build pipelines

## Solution Structure

```
RentalForge.slnx                          # XML-based solution file
src/RentalForge.Api/                      # ASP.NET Core Web API
  Controllers/                            # API controllers (HealthController)
  Data/
    DvdrentalContext.cs                    # EF Core DbContext
    Entities/                             # EF Core entity classes
    Migrations/                           # EF Core migrations
    ReferenceData/                        # HasData() seed configurations
    Seeding/                              # Dev data seeder + JSON seed files
  Models/                                 # DTOs / view models
  Program.cs                              # App entry point
tests/RentalForge.Api.Tests/              # xUnit test project
specs/                                    # Feature specifications (spec-kit)
```

## Build Commands

```bash
dotnet build                                           # build the solution
dotnet test                                            # run all tests
dotnet test --filter "FullyQualifiedName~TestName"     # run a single test
dotnet run --project src/RentalForge.Api               # run the API
dotnet run --project src/RentalForge.Api -- --seed     # run with dev data seeding
```

## Active Technologies

### Backend
- C# 14 / .NET 10.0 (LTS, patch 10.0.3)
- ASP.NET Core 10.0
- EF Core 10.0 + Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0
- Ardalis.Result 10.1.0 + Ardalis.Result.AspNetCore 10.1.0 + Ardalis.Result.FluentValidation 10.1.0
- Swashbuckle.AspNetCore 10.1.4 + Microsoft.AspNetCore.OpenApi 10.0.3

### Testing
- xUnit 2.9.3 + FluentAssertions 8.8.0
- Microsoft.AspNetCore.Mvc.Testing 10.0.3
- Testcontainers.PostgreSql 4.10.0

### Frontend (planned)
- React + TypeScript (strict mode)
- Testing library TBD (React Testing Library + Jest/Vitest per constitution)

## Key Constraints

- All API endpoints MUST use controller-based routing (no minimal APIs) per constitution v1.8.0
- Validation MUST aggregate all errors before responding — never early-return on first failure (constitution v1.8.0)
- TDD is NON-NEGOTIABLE — red-green-refactor for all production code
- AutoFixture MUST be used for anonymous test data generation
- Connection strings and secrets via `dotnet user-secrets` only — never committed
- Functional style with immutable data structures preferred (records, init-only properties)
- Service methods MUST return `Result<T>` / `Result` (Ardalis.Result) for expected outcomes (validation failures, not-found, business-rule violations); exceptions reserved for unexpected failures only
- DTOs MUST be flat and simple: return IDs for related entities (not embedded objects); inline related data as flat properties for one-level relationships; use nested structures only for multi-level relationships (constitution v1.8.0)
- Enum DTO properties MUST use a JSON converter accepting both numeric and string values (constitution v1.8.0)

## Recent Changes
- 004-customer-crud: Full Customer CRUD API (GET list/search/pagination, GET by ID, POST, PUT, DELETE soft-delete). FluentValidation.AspNetCore 11.3.1, AutoFixture 4.18.1, AutoFixture.Xunit2 4.18.1. Service layer (ICustomerService/CustomerService), controller-based routing, TDD with 90 tests passing.
- 005-result-pattern-refactor: Migrated from exception-based error handling (ServiceValidationException) to Ardalis.Result pattern. Service methods return Result<T>/Result; controllers use explicit result.Status switch expressions. FluentValidation moved from ASP.NET pipeline auto-validation into service layer for error aggregation via .AsErrors() bridge. ServiceValidationException deleted. All 93 tests pass unchanged (behavior-preserving refactoring).
