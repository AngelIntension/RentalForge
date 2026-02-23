# Quickstart: Film CRUD API

**Branch**: `006-film-crud` | **Date**: 2026-02-23

## Prerequisites

- .NET 10.0 SDK installed
- PostgreSQL 18 running at localhost:5432 with `dvdrental` database
- Docker (for Testcontainers in tests)
- Connection string configured via user-secrets:
  ```bash
  dotnet user-secrets set "ConnectionStrings:Dvdrental" \
    "Host=localhost;Port=5432;Database=dvdrental;Username=postgres;Password=yourpassword" \
    --project src/RentalForge.Api
  ```

## Build & Run

```bash
# Build the solution
dotnet build

# Run all tests (uses Testcontainers — Docker must be running)
dotnet test

# Run the API
dotnet run --project src/RentalForge.Api

# Run with dev data seeding
dotnet run --project src/RentalForge.Api -- --seed
```

## Try the Endpoints

```bash
# List films (default pagination)
curl http://localhost:5000/api/films

# Search by title
curl "http://localhost:5000/api/films?search=academy"

# Filter by category
curl "http://localhost:5000/api/films?category=Action"

# Filter by rating
curl "http://localhost:5000/api/films?rating=PG-13"

# Filter by year range
curl "http://localhost:5000/api/films?yearFrom=2000&yearTo=2010"

# Combine filters
curl "http://localhost:5000/api/films?search=love&category=Drama&rating=R&page=1&pageSize=5"

# Get film details (with actors, categories, language)
curl http://localhost:5000/api/films/1

# Create a film
curl -X POST http://localhost:5000/api/films \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Film",
    "languageId": 1,
    "rentalDuration": 5,
    "rentalRate": 3.99,
    "replacementCost": 24.99,
    "rating": "PG"
  }'

# Update a film
curl -X PUT http://localhost:5000/api/films/1001 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Film",
    "languageId": 1,
    "rentalDuration": 7,
    "rentalRate": 4.99,
    "replacementCost": 29.99
  }'

# Delete a film (will fail with 409 if film has inventory)
curl -X DELETE http://localhost:5000/api/films/1001
```

## Run Specific Tests

```bash
# All film tests
dotnet test --filter "FullyQualifiedName~Film"

# Only integration tests
dotnet test --filter "FullyQualifiedName~Integration.Film"

# Only validator unit tests
dotnet test --filter "FullyQualifiedName~CreateFilmValidator"
```

## Swagger UI

Navigate to `http://localhost:5000/swagger` to see the interactive
API documentation with all Film endpoints documented.

## Validation Checklist

After implementation, verify:
1. `dotnet build` succeeds with no warnings
2. `dotnet test` passes all tests (existing + new film tests)
3. Swagger UI shows all 5 Film endpoints with correct schemas
4. List endpoint returns lean DTOs (no actor/category/language names)
5. Detail endpoint returns flat related data (actors as string list)
6. Rating accepts both "PG-13" (string) and 2 (numeric) in requests
7. Delete returns 409 Conflict for films with inventory
8. All validation errors are aggregated (not early-return)
