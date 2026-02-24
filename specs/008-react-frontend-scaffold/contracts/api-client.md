# API Client Contract: 008-react-frontend-scaffold

**Date**: 2026-02-23

The frontend consumes the existing backend REST API through a centralized API client (`src/lib/api-client.ts`). This document defines the client's interface contract.

## API Client Interface

The API client is a single module that wraps `fetch` with:
- Configurable base URL (from environment variable)
- JSON content-type headers
- Response normalization (parse JSON, throw on non-2xx)
- Error normalization into `ApiError` shape

### Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| get | `get<T>(path: string, params?: Record<string, string>): Promise<T>` | GET request with optional query params |
| post | `post<T>(path: string, body: unknown): Promise<T>` | POST request with JSON body |
| put | `put<T>(path: string, body?: unknown): Promise<T>` | PUT request with optional JSON body |
| del | `del(path: string): Promise<void>` | DELETE request (no response body) |

### Error Handling

On non-2xx responses, the client throws an `ApiError` object:
- 400: Parses `ValidationProblemDetails` into `errors` field
- 404: Status 404 with title "Not Found"
- 409: Status 409 with title from ProblemDetails
- 5xx: Status with generic title

### Configuration

| Setting | Source | Default |
|---------|--------|---------|
| API Base URL | `VITE_API_BASE_URL` env var | `http://localhost:5000` |

---

## Consumed Endpoints

### Health

| Operation | Method | Path | Response |
|-----------|--------|------|----------|
| Check health | GET | `/health` | `HealthResponse` |

### Films

| Operation | Method | Path | Query Params | Request Body | Response |
|-----------|--------|------|-------------|-------------|----------|
| List films | GET | `/api/films` | search, category, rating, yearFrom, yearTo, page, pageSize | — | `PagedResponse<FilmListItem>` |
| Get film | GET | `/api/films/:id` | — | — | `FilmDetail` |

### Customers

| Operation | Method | Path | Query Params | Request Body | Response |
|-----------|--------|------|-------------|-------------|----------|
| List customers | GET | `/api/customers` | search, page, pageSize | — | `PagedResponse<CustomerListItem>` |
| Get customer | GET | `/api/customers/:id` | — | — | `CustomerListItem` |

### Rentals

| Operation | Method | Path | Query Params | Request Body | Response |
|-----------|--------|------|-------------|-------------|----------|
| List rentals | GET | `/api/rentals` | customerId, activeOnly, page, pageSize | — | `PagedResponse<RentalListItem>` |
| Get rental | GET | `/api/rentals/:id` | — | — | `RentalDetail` |
| Create rental | POST | `/api/rentals` | — | `CreateRentalRequest` | `RentalDetail` |
| Return rental | PUT | `/api/rentals/:id/return` | — | — | `RentalDetail` |

---

## TanStack Query Key Convention

| Resource | List Key | Detail Key | Purpose |
|----------|----------|------------|---------|
| Films | `['films', { search, category, rating, yearFrom, yearTo }]` | `['films', id]` | Invalidate all film lists: `queryKey: ['films']` |
| Customers | `['customers', { search }]` | `['customers', id]` | Invalidate all customer lists: `queryKey: ['customers']` |
| Rentals | `['rentals', { customerId, activeOnly }]` | `['rentals', id]` | Invalidate all rental lists: `queryKey: ['rentals']` |

### Mutation → Invalidation Map

| Mutation | Invalidates |
|----------|------------|
| Create rental | `['rentals']` (all rental queries) |
| Return rental | `['rentals']` (all rental queries) |

---

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `VITE_API_BASE_URL` | Backend API base URL | `http://localhost:5000` |

All `VITE_`-prefixed variables are exposed to client-side code by Vite. Sensitive values (API keys, secrets) MUST NOT use the `VITE_` prefix.
