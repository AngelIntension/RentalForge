# Quickstart: Rental CRUD API

**Branch**: `007-rental-crud` | **Date**: 2026-02-23

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
# List rentals (default pagination, most recent first)
curl http://localhost:5000/api/rentals

# Filter by customer
curl "http://localhost:5000/api/rentals?customerId=130"

# Filter active rentals only
curl "http://localhost:5000/api/rentals?activeOnly=true"

# Combine filters with pagination
curl "http://localhost:5000/api/rentals?customerId=130&activeOnly=true&page=1&pageSize=5"

# Get rental details (with customer name, film title, staff name)
curl http://localhost:5000/api/rentals/1

# Create a rental (filmId + storeId → resolves inventory)
curl -X POST http://localhost:5000/api/rentals \
  -H "Content-Type: application/json" \
  -d '{
    "filmId": 80,
    "storeId": 1,
    "customerId": 130,
    "staffId": 1
  }'

# Return a rental
curl -X PUT http://localhost:5000/api/rentals/16050/return

# Delete a rental (will fail with 409 if rental has payments)
curl -X DELETE http://localhost:5000/api/rentals/16050
```

## Run Specific Tests

```bash
# All rental tests
dotnet test --filter "FullyQualifiedName~Rental"

# Only integration tests
dotnet test --filter "FullyQualifiedName~Integration.Rental"

# Only validator unit tests
dotnet test --filter "FullyQualifiedName~CreateRentalValidator"
```

## Swagger UI

Navigate to `http://localhost:5000/swagger` to see the interactive
API documentation with all Rental endpoints documented.

## Validation Checklist

After implementation, verify:
1. `dotnet build` succeeds with no warnings
2. `dotnet test` passes all tests (existing + new rental tests)
3. Swagger UI shows all 5 Rental endpoints with correct schemas
4. List endpoint returns lean DTOs (IDs only, no names)
5. Detail endpoint returns flat related data (customer name, film title, staff name)
6. Create with filmId + storeId resolves to an inventory copy
7. Create with unavailable film returns specific error (not stocked vs all rented)
8. Return endpoint sets return date; rejects already-returned rentals
9. Delete returns 409 Conflict for rentals with payments
10. All validation errors are aggregated (not early-return)
