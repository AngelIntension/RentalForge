# Data Model: 008-react-frontend-scaffold

**Date**: 2026-02-23
**Branch**: `008-react-frontend-scaffold`

The frontend does not define its own persistent data model — all data originates from the backend API. This document defines the **TypeScript type definitions** that mirror the backend API response DTOs, used by the centralized API client and TanStack Query hooks.

## Shared API Types

### PagedResponse\<T\>

Generic pagination wrapper returned by all list endpoints.

| Field | Type | Description |
|-------|------|-------------|
| items | T[] | Array of items for the current page |
| page | number | Current page number (1-based) |
| pageSize | number | Items per page |
| totalCount | number | Total matching items across all pages |
| totalPages | number | Total number of pages |

### ApiError

Normalized error shape for frontend error handling.

| Field | Type | Description |
|-------|------|-------------|
| status | number | HTTP status code |
| title | string | Error title |
| errors | Record\<string, string[]\> \| null | Field-level validation errors (from ValidationProblemDetails) |

---

## Film Types

### FilmListItem

Lean DTO for film list views (from `GET /api/films`).

| Field | Type | Description |
|-------|------|-------------|
| id | number | Film ID |
| title | string | Film title |
| description | string \| null | Film description |
| releaseYear | number \| null | Release year |
| languageId | number | Language ID |
| originalLanguageId | number \| null | Original language ID |
| rentalDuration | number | Rental duration in days |
| rentalRate | number | Rental rate (decimal) |
| length | number \| null | Film length in minutes |
| replacementCost | number | Replacement cost (decimal) |
| rating | MpaaRating \| null | MPAA rating enum |
| specialFeatures | string[] \| null | Special features list |
| lastUpdate | string | ISO 8601 timestamp |

### FilmDetail

Rich DTO for film detail view (from `GET /api/films/:id`).

Extends FilmListItem with:

| Field | Type | Description |
|-------|------|-------------|
| languageName | string | Language name (flat) |
| originalLanguageName | string \| null | Original language name |
| actors | string[] | Actor full names |
| categories | string[] | Category names |

### MpaaRating

Enum values (string): `"G"`, `"PG"`, `"PG-13"`, `"R"`, `"NC-17"`

---

## Customer Types

### CustomerListItem

DTO for customer list and detail views (from `GET /api/customers` and `GET /api/customers/:id`).

| Field | Type | Description |
|-------|------|-------------|
| id | number | Customer ID |
| storeId | number | Assigned store ID |
| firstName | string | First name |
| lastName | string | Last name |
| email | string \| null | Email address |
| addressId | number | Address ID |
| isActive | boolean | Active status |
| createDate | string | ISO 8601 date |
| lastUpdate | string | ISO 8601 timestamp |

---

## Rental Types

### RentalListItem

Lean DTO for rental list views (from `GET /api/rentals`).

| Field | Type | Description |
|-------|------|-------------|
| id | number | Rental ID |
| rentalDate | string | ISO 8601 timestamp |
| returnDate | string \| null | ISO 8601 timestamp (null = active) |
| inventoryId | number | Inventory ID |
| customerId | number | Customer ID |
| staffId | number | Staff ID |
| lastUpdate | string | ISO 8601 timestamp |

### RentalDetail

Rich DTO for rental detail view (from `GET /api/rentals/:id`).

| Field | Type | Description |
|-------|------|-------------|
| id | number | Rental ID |
| rentalDate | string | ISO 8601 timestamp |
| returnDate | string \| null | ISO 8601 timestamp |
| inventoryId | number | Inventory ID |
| filmId | number | Film ID |
| filmTitle | string | Film title (flat) |
| storeId | number | Store ID |
| customerId | number | Customer ID |
| customerFirstName | string | Customer first name (flat) |
| customerLastName | string | Customer last name (flat) |
| staffId | number | Staff ID |
| staffFirstName | string | Staff first name (flat) |
| staffLastName | string | Staff last name (flat) |
| lastUpdate | string | ISO 8601 timestamp |

### CreateRentalRequest

Request body for `POST /api/rentals`.

| Field | Type | Validation |
|-------|------|------------|
| filmId | number | Required, positive integer |
| storeId | number | Required, positive integer |
| customerId | number | Required, positive integer |
| staffId | number | Required, positive integer |

**Zod schema**: `z.object` with `z.coerce.number().int().positive()` for each field. TypeScript type derived via `z.infer<typeof createRentalSchema>`.

---

## Query Parameter Types

### FilmSearchParams

Query parameters for `GET /api/films`.

| Field | Type | Default |
|-------|------|---------|
| search | string \| undefined | — |
| category | string \| undefined | — |
| rating | MpaaRating \| undefined | — |
| yearFrom | number \| undefined | — |
| yearTo | number \| undefined | — |
| page | number | 1 |
| pageSize | number | 10 |

### CustomerSearchParams

Query parameters for `GET /api/customers`.

| Field | Type | Default |
|-------|------|---------|
| search | string \| undefined | — |
| page | number | 1 |
| pageSize | number | 10 |

### RentalSearchParams

Query parameters for `GET /api/rentals`.

| Field | Type | Default |
|-------|------|---------|
| customerId | number \| undefined | — |
| activeOnly | boolean | false |
| page | number | 1 |
| pageSize | number | 10 |

---

## State Transitions

### Rental Lifecycle (read-only from frontend perspective)

```
[No rental] → POST /api/rentals → Active (returnDate = null)
Active → PUT /api/rentals/:id/return → Returned (returnDate set)
```

### Theme State

```
System default → User toggles → Light | Dark
Persisted to localStorage key "rentalforge-theme"
```
