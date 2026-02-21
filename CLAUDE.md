# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RentalForge is a C# project for learning spec-driven AI development in Linux using GitHub spec-kit and Claude Code.

## Build Commands

No solution or project files exist yet. Once created, standard .NET CLI commands apply:

- `dotnet build` — build the solution
- `dotnet test` — run all tests
- `dotnet test --filter "FullyQualifiedName~TestName"` — run a single test
- `dotnet run --project <ProjectName>` — run a specific project

## Active Technologies
- C# 14 / .NET 10.0 (LTS, patch 10.0.3) + ASP.NET Core + EF Core 10.0, Npgsql.EntityFrameworkCore.PostgreSQL, Swashbuckle.AspNetCore, Microsoft.AspNetCore.OpenApi (001-efcore-health-api)
- PostgreSQL 18 (existing `dvdrental` sample database at localhost:5432) (001-efcore-health-api)
- All API endpoints MUST use controller-based routing (no minimal APIs) per constitution v1.3.0

## Recent Changes
- 001-efcore-health-api: Added C# 14 / .NET 10.0 (LTS, patch 10.0.3) + EF Core 10.0, Npgsql.EntityFrameworkCore.PostgreSQL, Swashbuckle.AspNetCore, Microsoft.AspNetCore.OpenApi
