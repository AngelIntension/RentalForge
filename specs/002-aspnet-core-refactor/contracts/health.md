# Contract: Health Endpoint

**Branch**: `002-aspnet-core-refactor` | **Date**: 2026-02-21

## Overview

The health endpoint contract is **unchanged** by this refactor.
The URL, HTTP method, request/response shapes, status codes, and
content types remain identical. Only the internal implementation
pattern changes (minimal API → controller).

## Endpoint

**Method**: GET
**Path**: `/health`
**Authentication**: None
**Content-Type**: `application/json`

## Responses

### 200 OK — Database is healthy and reachable

```json
{
  "status": "healthy",
  "databaseVersion": "PostgreSQL 18.x on ...",
  "serverTime": "2026-02-21T12:00:00+00:00",
  "error": null
}
```

### 503 Service Unavailable — Database is unhealthy or unreachable

```json
{
  "status": "unhealthy",
  "databaseVersion": null,
  "serverTime": null,
  "error": "Database connection failed: <error message>"
}
```

## OpenAPI Metadata

| Property     | Value                                  |
|--------------|----------------------------------------|
| OperationId  | HealthCheck                            |
| Summary      | Database health check                  |
| 200 Desc     | Database is healthy and reachable      |
| 503 Desc     | Database is unhealthy or unreachable   |

## Implementation Change (internal only)

| Aspect         | Before (minimal API)              | After (controller)                         |
|----------------|-----------------------------------|--------------------------------------------|
| Registration   | `app.MapHealthEndpoint()`         | `app.MapControllers()` (auto-discovery)    |
| Location       | `Endpoints/HealthEndpoint.cs`     | `Controllers/HealthController.cs`          |
| DTO location   | `Endpoints/HealthEndpoint.cs`     | `Models/HealthResponse.cs`                 |
| OpenAPI config | Fluent API (`.WithName()`, etc.)  | Attributes (`[SwaggerOperation]`, etc.)    |
| Route          | `app.MapGet("/health", ...)`      | `[HttpGet("health")]` on controller action |
