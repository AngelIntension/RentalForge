# Contract: Health Endpoint

**Endpoint**: `GET /health`
**Authentication**: None (public diagnostic endpoint)
**Content-Type**: `application/json`

## Request

No request body. No query parameters. No headers required.

```
GET /health HTTP/1.1
Host: localhost:{port}
```

## Response: 200 OK (Healthy)

Returned when the database is reachable and responds to queries.

```json
{
  "status": "healthy",
  "databaseVersion": "PostgreSQL 18.x on x86_64-pc-linux-gnu ...",
  "serverTime": "2026-02-21T14:30:00.000000+00:00"
}
```

| Field | Type | Description |
|-------|------|-------------|
| status | string | Always `"healthy"` for 200 responses |
| databaseVersion | string | Result of PostgreSQL `SELECT version()` |
| serverTime | string (ISO 8601) | Result of PostgreSQL `SELECT NOW()` |

## Response: 503 Service Unavailable (Unhealthy)

Returned when the database is unreachable or the query fails.

```json
{
  "status": "unhealthy",
  "error": "Database connection failed: could not connect to server"
}
```

| Field | Type | Description |
|-------|------|-------------|
| status | string | Always `"unhealthy"` for 503 responses |
| error | string | Human-readable error description |

**Note**: The `databaseVersion` and `serverTime` fields are absent
from the 503 response. The `error` field is absent from the 200
response.

## OpenAPI Metadata

The endpoint MUST appear in the Swagger UI and OpenAPI spec with:
- Operation summary: "Database health check"
- 200 response description: "Database is healthy and reachable"
- 503 response description: "Database is unhealthy or unreachable"
- Both response schemas documented with example values

## Performance Contract

- 200 response: MUST return within 2 seconds under normal
  conditions.
- 503 response: MUST return within 5 seconds (includes connection
  timeout).
