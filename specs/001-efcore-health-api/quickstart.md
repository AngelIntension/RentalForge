# Quickstart: EF Core Scaffold and Health API

**Feature Branch**: `001-efcore-health-api`

## Prerequisites

- .NET 10 SDK (10.0.3+)
- PostgreSQL with the `dvdrental` database restored at
  `localhost:5432`
- Docker (for running integration tests via Testcontainers)
- `dotnet-ef` global tool:
  `dotnet tool install --global dotnet-ef`

## Setup

### 1. Clone and switch to the feature branch

```bash
git clone <repo-url>
cd RentalForge
git checkout 001-efcore-health-api
```

### 2. Restore the dvdrental database (if not already done)

Download from https://www.postgresqltutorial.com/postgresql-getting-started/postgresql-sample-database/
and restore:

```bash
pg_restore -U postgres -d dvdrental dvdrental.tar
```

### 3. Configure the connection string

Initialize user-secrets and set the connection string:

```bash
cd src/RentalForge.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Dvdrental" \
  "Host=localhost;Port=5432;Database=dvdrental;Username=postgres;Password=<your-password>"
```

### 4. Build

```bash
dotnet build
```

### 5. Run the API

```bash
dotnet run --project src/RentalForge.Api
```

### 6. Verify the health endpoint

```bash
curl http://localhost:5000/health
```

Expected response:
```json
{
  "status": "healthy",
  "databaseVersion": "PostgreSQL 18.x ...",
  "serverTime": "2026-02-21T14:30:00.000000+00:00"
}
```

### 7. View API documentation

Open in browser: `http://localhost:5000/swagger`

### 8. Run integration tests

Ensure Docker is running, then:

```bash
dotnet test
```

The test provisions its own PostgreSQL container — no external
database required.

## Project Structure

```text
RentalForge.sln
src/
└── RentalForge.Api/
    ├── Program.cs                 # Minimal API entry point
    ├── appsettings.json           # Non-sensitive config
    ├── Data/
    │   ├── DvdrentalContext.cs     # Scaffolded DbContext
    │   └── Entities/              # Scaffolded entity classes
    └── Endpoints/
        └── HealthEndpoint.cs      # /health endpoint

tests/
└── RentalForge.Api.Tests/
    ├── Infrastructure/
    │   └── TestWebAppFactory.cs   # WebApplicationFactory + Testcontainer
    └── Integration/
        └── HealthEndpointTests.cs # /health integration test
```

## Common Issues

| Issue | Solution |
|-------|----------|
| Connection string not found | Run `dotnet user-secrets set` (step 3) |
| App fails to start | Check PostgreSQL is running on localhost:5432 |
| Tests fail with "Docker not found" | Start Docker: `sudo service docker start` |
| Slow test startup on /mnt/ | Expected on WSL2; native Linux paths are faster |
