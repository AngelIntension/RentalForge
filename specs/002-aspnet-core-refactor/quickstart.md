# Quickstart: ASP.NET Core Controller Refactor

**Branch**: `002-aspnet-core-refactor` | **Date**: 2026-02-21

## Prerequisites

- .NET 10.0 SDK (LTS, patch 10.0.3 or later)
- PostgreSQL 18 with `dvdrental` database at localhost:5432
- Docker (for Testcontainers in integration tests)
- User secrets configured for the API project

## Setup

1. **Check out the branch**:

   ```bash
   git checkout 002-aspnet-core-refactor
   ```

2. **Verify user secrets** (if not already configured):

   ```bash
   cd src/RentalForge.Api
   dotnet user-secrets set "ConnectionStrings:Dvdrental" \
     "Host=localhost;Port=5432;Database=dvdrental;Username=postgres;Password=<your-password>"
   ```

## Build

```bash
dotnet build
```

## Run

```bash
dotnet run --project src/RentalForge.Api
```

The API starts on `http://localhost:5089` (HTTP) or
`https://localhost:7269` (HTTPS).

## Verify

1. **Health endpoint**:

   ```bash
   curl -s http://localhost:5089/health | jq .
   ```

   Expected (healthy):
   ```json
   {
     "status": "healthy",
     "databaseVersion": "PostgreSQL 18...",
     "serverTime": "2026-02-21T...",
     "error": null
   }
   ```

2. **Swagger UI**: Open `http://localhost:5089/swagger` in a
   browser. Verify the health endpoint appears with operation
   name "HealthCheck" and summary "Database health check".

## Test

```bash
dotnet test
```

All 3 integration tests must pass:
- `HealthEndpoint_ReturnsOk_WhenDatabaseIsReachable`
- `HealthEndpoint_Returns503_WhenDatabaseIsUnreachable`
- `App_FailsFast_WhenConnectionStringMissing`

Note: Integration tests use Testcontainers and require Docker
to be running. They do NOT require the local PostgreSQL instance.

## Verification Checklist

- [ ] `dotnet build` succeeds with zero warnings
- [ ] `dotnet test` passes all 3 tests
- [ ] `GET /health` returns 200 with healthy payload
- [ ] Swagger UI shows health endpoint with correct metadata
- [ ] No `Endpoints/` directory exists in `src/RentalForge.Api/`
- [ ] No `app.MapGet`/`app.MapPost` calls for business endpoints
      in `Program.cs`
