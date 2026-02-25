# RentalForge

A full-stack rental management application built as a learning project for spec-driven AI development using GitHub spec-kit and Claude Code.

## What is this?

RentalForge is a Single Page Application (SPA) with a React/TypeScript frontend and a C# ASP.NET Core RESTful API backend, built against PostgreSQL's classic `dvdrental` sample database. The project explores how to ship real features using a spec-first workflow where every change starts as a specification before any code is written.

## Architecture

| Layer | Technology |
|-------|-----------|
| Frontend | React 19 + TypeScript (strict mode) + Vite |
| UI | Tailwind CSS + Radix UI + Shadcn UI |
| State | TanStack React Query |
| Routing | React Router 7 |
| Backend API | ASP.NET Core 10.0 (controller-based routing) |
| Auth | ASP.NET Core Identity + JWT bearer tokens |
| Validation | FluentValidation (backend) + Zod v4 (frontend) |
| ORM | Entity Framework Core 10.0 + Npgsql |
| Database | PostgreSQL 18 (`dvdrental` sample DB) |
| API Docs | Swagger / OpenAPI |

The frontend and backend live in a single monorepo with independent build pipelines.

## Features

- **Customer management** — full CRUD with search and pagination
- **Film catalog** — CRUD with multi-table search, category/rating/year filters
- **Rental tracking** — CRUD with inventory management
- **Authentication** — JWT login/register with refresh token rotation and rate limiting
- **Protected routes** — role-based access control (Admin, Staff, Customer)
- **Dark mode** — theme toggle with system preference detection
- **PWA support** — progressive web app with service worker

## Development Approach

All development follows a strict spec-driven workflow governed by a [project constitution](.specify/memory/constitution.md):

1. **Specify** — write a feature spec with prioritized user stories
2. **Clarify** — resolve ambiguities via targeted questions
3. **Plan** — produce an implementation plan with research and design artifacts
4. **Tasks** — generate a dependency-ordered task list
5. **Implement** — TDD (red-green-refactor) following the task list

Key principles: spec-first, test-first (non-negotiable), clean architecture, YAGNI, functional style with immutability.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (LTS)
- [Node.js](https://nodejs.org/) (LTS) + npm
- [PostgreSQL 18](https://www.postgresql.org/) with the [`dvdrental`](https://www.postgresqltutorial.com/postgresql-getting-started/postgresql-sample-database/) sample database loaded
- Linux or WSL2
- Docker (for running integration tests via Testcontainers)

## Getting Started

### 1. Configure secrets

Connection strings and JWT signing keys are managed via `dotnet user-secrets` and are never committed to source control.

```bash
# Set the database connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=dvdrental;Username=postgres;Password=yourpassword" \
  --project src/RentalForge.Api

# Set JWT configuration
dotnet user-secrets set "Jwt:Key" "your-256-bit-secret-key-here-min-32-chars" \
  --project src/RentalForge.Api
dotnet user-secrets set "Jwt:Issuer" "RentalForge" \
  --project src/RentalForge.Api
dotnet user-secrets set "Jwt:Audience" "RentalForge" \
  --project src/RentalForge.Api
```

### 2. Run the backend

```bash
dotnet build                                        # build the solution
dotnet run --project src/RentalForge.Api             # start the API (http://localhost:5089)
dotnet run --project src/RentalForge.Api -- --seed   # start with dev data seeding
```

The API serves Swagger UI at [http://localhost:5089/swagger](http://localhost:5089/swagger) in development mode.

### 3. Run the frontend

```bash
cd src/RentalForge.Web
npm install                                          # install dependencies
npm run dev                                          # start Vite dev server (http://localhost:5173)
```

The frontend proxies API requests to the backend at `http://localhost:5089`.

## Running Tests

### Backend tests (~206 tests)

Backend tests use xUnit with Testcontainers (Docker required for integration tests).

```bash
dotnet test                                          # run all backend tests
dotnet test --filter "FullyQualifiedName~TestName"   # run a specific test
dotnet test --verbosity normal                       # run with detailed output
```

### Frontend tests (~91 tests)

Frontend tests use Vitest with jsdom and MSW for API mocking.

```bash
cd src/RentalForge.Web
npm run test                                         # run all frontend tests
npm run test:watch                                   # run in watch mode
npm run test:coverage                                # run with coverage report
```

### Other checks

```bash
cd src/RentalForge.Web
npm run typecheck                                    # TypeScript type checking
npm run lint                                         # ESLint
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and receive JWT + refresh token |
| POST | `/api/auth/refresh` | Refresh an expired access token |
| POST | `/api/auth/logout` | Revoke refresh token |
| GET | `/api/customers` | List customers (search, pagination) |
| POST | `/api/customers` | Create a customer |
| GET | `/api/customers/{id}` | Get customer by ID |
| PUT | `/api/customers/{id}` | Update a customer |
| DELETE | `/api/customers/{id}` | Soft-delete a customer |
| GET | `/api/films` | List films (search, filter, pagination) |
| POST | `/api/films` | Create a film |
| GET | `/api/films/{id}` | Get film detail |
| PUT | `/api/films/{id}` | Update a film |
| DELETE | `/api/films/{id}` | Delete a film |
| GET | `/api/rentals` | List rentals (pagination) |
| POST | `/api/rentals` | Create a rental |
| GET | `/api/rentals/{id}` | Get rental detail |
| PUT | `/api/rentals/{id}` | Update a rental |
| DELETE | `/api/rentals/{id}` | Delete a rental |

## Project Structure

```
RentalForge/
├── src/
│   ├── RentalForge.Api/              # C# ASP.NET Core Web API
│   │   ├── Controllers/              # API controllers
│   │   ├── Services/                 # Business logic layer
│   │   ├── Validators/               # FluentValidation rules
│   │   ├── Data/                     # EF Core DbContext, entities, migrations
│   │   ├── Models/                   # DTOs and view models
│   │   └── Program.cs               # App entry point with DI configuration
│   └── RentalForge.Web/             # React/TypeScript SPA
│       ├── src/
│       │   ├── app/                  # Root layout, routes, providers
│       │   ├── components/           # UI components (auth, layout, domain, shared)
│       │   ├── hooks/                # Custom React hooks (auth, data, theme)
│       │   ├── lib/                  # API client, query client, validators
│       │   ├── pages/                # Route pages
│       │   ├── types/                # TypeScript type definitions
│       │   └── test/                 # Test setup, fixtures, MSW mocks
│       ├── vite.config.ts            # Vite bundler configuration
│       └── vitest.config.ts          # Vitest test runner configuration
├── tests/
│   └── RentalForge.Api.Tests/        # xUnit backend test suite
│       ├── Unit/                     # Validator unit tests
│       ├── Integration/              # Endpoint integration tests (Testcontainers)
│       └── Infrastructure/           # Test helpers and factories
├── specs/                            # Feature specifications (spec-kit)
├── postman/                          # Postman collections and environments
├── .specify/                         # Spec-kit configuration and constitution
└── RentalForge.slnx                  # .NET solution file
```

## Project Status

This is an active learning project. Features are added incrementally via the spec-kit workflow. See the `specs/` directory for completed and in-progress feature specifications.

### Completed features

| # | Feature | Description |
|---|---------|-------------|
| 001 | EF Core + Health API | EF Core setup with PostgreSQL, health endpoint |
| 002 | ASP.NET Core Refactor | Project restructure and architecture cleanup |
| 003 | DB Schema Seeding | HasData() reference data + DevDataSeeder CLI tool |
| 004 | Customer CRUD | Full Customer API with search, pagination, soft-delete |
| 005 | Result Pattern Refactor | Exception-based → Ardalis.Result pattern migration |
| 006 | Film CRUD | Film API with multi-table search, filters, MpaaRating enum |
| 007 | Rental CRUD | Full Rental API with inventory management |
| 008 | React Frontend Scaffold | React SPA with Vite, Tailwind, Shadcn UI, routing, data hooks |
| 009 | Auth System | JWT authentication, refresh tokens, protected routes, rate limiting |
