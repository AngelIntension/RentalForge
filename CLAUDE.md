# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RentalForge is a full-stack SPA rental management application for learning spec-driven AI development in Linux using GitHub spec-kit and Claude Code. The backend is a C# ASP.NET Core RESTful API and the frontend is a React/TypeScript Single Page Application, both in a monorepo.

## Architecture

- **Backend**: C# ASP.NET Core RESTful API with controller-based routing (no minimal APIs) per constitution v1.5.0
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
- Swashbuckle.AspNetCore 10.1.4 + Microsoft.AspNetCore.OpenApi 10.0.3

### Testing
- xUnit 2.9.3 + FluentAssertions 8.8.0
- Microsoft.AspNetCore.Mvc.Testing 10.0.3
- Testcontainers.PostgreSql 4.10.0

### Frontend (planned)
- React + TypeScript (strict mode)
- Testing library TBD (React Testing Library + Jest/Vitest per constitution)

## Key Constraints

- All API endpoints MUST use controller-based routing (no minimal APIs) per constitution v1.5.0
- TDD is NON-NEGOTIABLE — red-green-refactor for all production code
- AutoFixture MUST be used for anonymous test data generation
- Connection strings and secrets via `dotnet user-secrets` only — never committed
- Functional style with immutable data structures preferred (records, init-only properties)

## Recent Changes
- 004-customer-crud: Added C# 14 / .NET 10.0 (LTS, patch 10.0.3) + ASP.NET Core 10.0, EF Core 10.0 + Npgsql 10.0.0, FluentValidation 11.11, Swashbuckle 10.1.4
