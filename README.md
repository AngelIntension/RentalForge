# RentalForge

A full-stack rental management application built as a learning project for spec-driven AI development using GitHub spec-kit and Claude Code.

## What is this?

RentalForge is a Single Page Application (SPA) with a React/TypeScript frontend and a C# ASP.NET Core RESTful API backend, built against PostgreSQL's classic `dvdrental` sample database. The project explores how to ship real features using a spec-first workflow where every change starts as a specification before any code is written.

## Architecture

| Layer | Technology |
|-------|-----------|
| Frontend | React + TypeScript (strict mode) |
| Backend API | ASP.NET Core (controller-based routing) |
| ORM | Entity Framework Core + Npgsql |
| Database | PostgreSQL 18 (`dvdrental` sample DB) |
| Auth | ASP.NET Core Identity + JWT bearer tokens |
| Roles | Admin, Staff, Customer |
| API Docs | Swagger / OpenAPI |

The frontend and backend live in a single monorepo with independent build pipelines.

## Development Approach

All development follows a strict spec-driven workflow governed by a [project constitution](.specify/memory/constitution.md):

1. **Specify** — write a feature spec with prioritized user stories
2. **Clarify** — resolve ambiguities via targeted questions
3. **Plan** — produce an implementation plan with research and design artifacts
4. **Tasks** — generate a dependency-ordered task list
5. **Implement** — TDD (red-green-refactor) following the task list

Key principles: spec-first, test-first (non-negotiable), clean architecture, YAGNI, functional style with immutability.

## Prerequisites

- .NET 10.0 SDK (LTS)
- Node.js (LTS)
- PostgreSQL 18 with the [`dvdrental`](https://www.postgresqltutorial.com/postgresql-getting-started/postgresql-sample-database/) sample database loaded
- Linux or WSL2

## Getting Started

```bash
# Backend
dotnet build
dotnet test
dotnet run --project src/RentalForge.Api

# Frontend (once scaffolded)
cd src/RentalForge.Web
npm install
npm run dev
```

Connection strings and JWT signing keys are managed via `dotnet user-secrets` and are never committed to source control.

## Project Status

This is an active learning project. Features are added incrementally via the spec-kit workflow. See the `specs/` directory for completed and in-progress feature specifications.
