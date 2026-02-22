# Quickstart: Customer CRUD API

**Feature**: 004-customer-crud

## Prerequisites

- .NET 10.0 SDK installed
- Docker running (for Testcontainers in tests)
- PostgreSQL 18 running at localhost:5432 with dvdrental database (for manual testing)
- User secrets configured:
  ```bash
  cd src/RentalForge.Api
  dotnet user-secrets set "ConnectionStrings:Dvdrental" \
    "Host=localhost;Port=5432;Database=dvdrental;Username=postgres;Password=<your-password>"
  ```

## Build

```bash
dotnet build
```

## Run Tests

```bash
# All tests (requires Docker for Testcontainers)
dotnet test

# Run only customer tests
dotnet test --filter "FullyQualifiedName~Customer"
```

## Run the API

```bash
# Start the API server
dotnet run --project src/RentalForge.Api

# With dev data seeding (populates ~599 customers)
dotnet run --project src/RentalForge.Api -- --seed
```

## Test Endpoints

```bash
# List customers (default pagination)
curl http://localhost:5000/api/customers

# Search by name
curl "http://localhost:5000/api/customers?search=mary&page=1&pageSize=5"

# Get customer by ID
curl http://localhost:5000/api/customers/1

# Create customer
curl -X POST http://localhost:5000/api/customers \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe","email":"john@example.com","storeId":1,"addressId":1}'

# Update customer
curl -X PUT http://localhost:5000/api/customers/1 \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Mary","lastName":"Johnson","email":"mary.j@example.com","storeId":1,"addressId":1}'

# Deactivate customer (soft delete)
curl -X DELETE http://localhost:5000/api/customers/1
```

## Swagger UI

Browse to `http://localhost:5000/swagger` when running in Development mode.

## New Dependencies

This feature adds to the API project:
- `FluentValidation.AspNetCore` — declarative input validation

This feature adds to the test project:
- `AutoFixture` — anonymous test data generation (constitution mandate)
- `AutoFixture.Xunit2` — xUnit integration for [AutoData]
