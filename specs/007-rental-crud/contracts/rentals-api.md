# API Contract: Rentals

**Branch**: `007-rental-crud` | **Date**: 2026-02-23
**Base path**: `/api/rentals`

---

## GET /api/rentals

**Summary**: List rentals with optional filtering by customer and active status, plus pagination.

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| customerId | int | No | null | Exact match on customer identifier |
| activeOnly | bool | No | false | When true, only rentals with no return date |
| page | int | No | 1 | Page number (>= 1) |
| pageSize | int | No | 10 | Items per page (1-100, capped at 100) |

All filters combine with AND logic.

**Sort order**: Rental date descending (most recent first).

### Responses

**200 OK**
```json
{
  "items": [
    {
      "id": 1,
      "rentalDate": "2026-02-15T09:46:27Z",
      "returnDate": null,
      "inventoryId": 367,
      "customerId": 130,
      "staffId": 1,
      "lastUpdate": "2026-02-15T09:46:27Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 16044,
  "totalPages": 1605
}
```

**400 Bad Request** (invalid page/pageSize)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "page": ["'Page' must be greater than or equal to '1'."]
  }
}
```

---

## GET /api/rentals/{id}

**Summary**: Get full rental details by ID, including customer name, film title, and staff name.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | Rental identifier |

### Responses

**200 OK**
```json
{
  "id": 1,
  "rentalDate": "2026-02-15T09:46:27Z",
  "returnDate": "2026-02-17T14:30:00Z",
  "inventoryId": 367,
  "filmId": 80,
  "filmTitle": "Blanket Beverly",
  "storeId": 1,
  "customerId": 130,
  "customerFirstName": "Charlotte",
  "customerLastName": "Hunter",
  "staffId": 1,
  "staffFirstName": "Mike",
  "staffLastName": "Hillyer",
  "lastUpdate": "2026-02-17T14:30:00Z"
}
```

**404 Not Found** (rental does not exist)

---

## POST /api/rentals

**Summary**: Create a new rental. Accepts filmId + storeId; the system resolves an available inventory copy.

### Request Body

```json
{
  "filmId": 80,
  "storeId": 1,
  "customerId": 130,
  "staffId": 1
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| filmId | int | Yes | > 0, must reference existing film |
| storeId | int | Yes | > 0, must reference existing store |
| customerId | int | Yes | > 0, must reference existing, active customer |
| staffId | int | Yes | > 0, must reference existing, active staff member |

### Responses

**201 Created**
- Body: `RentalDetailResponse` (same shape as GET /api/rentals/{id})
- Header: `Location: /api/rentals/{newId}`
- The response includes the resolved `inventoryId` and flat display fields.

**400 Bad Request** (validation errors — field validation + business rules)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "filmId": ["Film with ID 999 does not exist."],
    "customerId": ["Customer with ID 888 does not exist or is inactive."]
  }
}
```

**400 Bad Request** (no available inventory)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "filmId": ["All copies of film 'Blanket Beverly' at store 1 are currently rented out."]
  }
}
```

**400 Bad Request** (film not stocked at store)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "filmId": ["Film 'Blanket Beverly' is not stocked at store 2."]
  }
}
```

---

## PUT /api/rentals/{id}/return

**Summary**: Process a rental return. Sets the return date to the current timestamp.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | Rental identifier |

### Request Body

None. No body required.

### Responses

**200 OK**
- Body: `RentalDetailResponse` (updated rental with return date set)

```json
{
  "id": 1,
  "rentalDate": "2026-02-15T09:46:27Z",
  "returnDate": "2026-02-23T10:15:00Z",
  "inventoryId": 367,
  "filmId": 80,
  "filmTitle": "Blanket Beverly",
  "storeId": 1,
  "customerId": 130,
  "customerFirstName": "Charlotte",
  "customerLastName": "Hunter",
  "staffId": 1,
  "staffFirstName": "Mike",
  "staffLastName": "Hillyer",
  "lastUpdate": "2026-02-23T10:15:00Z"
}
```

**400 Bad Request** (rental already returned)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "rentalId": ["Rental with ID 1 has already been returned."]
  }
}
```

**404 Not Found** (rental does not exist)

---

## DELETE /api/rentals/{id}

**Summary**: Permanently delete a rental (hard delete). Intended for administrators only (authorization not yet enforced).

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | Rental identifier |

### Responses

**204 No Content** (success — rental permanently deleted)

**404 Not Found** (rental does not exist)

**409 Conflict** (rental has associated payment records)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Cannot delete rental with ID 1 because it has associated payment records."
}
```
