# API Contract: Customers

**Base Path**: `/api/customers`
**Content-Type**: `application/json`

## GET /api/customers

List active customers with optional search and pagination.

**Query Parameters**:

| Parameter | Type | Default | Constraints | Description |
|-----------|------|---------|-------------|-------------|
| search | string? | null | — | Case-insensitive partial match on firstName, lastName, or email (OR logic) |
| page | int | 1 | >= 1 | Page number (1-based) |
| pageSize | int | 10 | 1–100 | Items per page; values > 100 capped to 100 |

**Response 200**:
```json
{
  "items": [
    {
      "id": 1,
      "storeId": 1,
      "firstName": "Mary",
      "lastName": "Smith",
      "email": "mary.smith@example.org",
      "addressId": 5,
      "isActive": true,
      "createDate": "2006-02-14",
      "lastUpdate": "2013-05-26T14:49:45.738"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 599,
  "totalPages": 60
}
```

**Response 400** (invalid page/pageSize):
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "page": ["'Page' must be greater than or equal to '1'."],
    "pageSize": ["'Page Size' must be between 1 and 100."]
  }
}
```

**Sort Order**: lastName ascending, then firstName ascending.

---

## GET /api/customers/{id}

Get a single active customer by ID.

**Path Parameters**:

| Parameter | Type | Description |
|-----------|------|-------------|
| id | int | Customer identifier |

**Response 200**:
```json
{
  "id": 1,
  "storeId": 1,
  "firstName": "Mary",
  "lastName": "Smith",
  "email": "mary.smith@example.org",
  "addressId": 5,
  "isActive": true,
  "createDate": "2006-02-14",
  "lastUpdate": "2013-05-26T14:49:45.738"
}
```

**Response 404** (not found or deactivated):
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404
}
```

---

## POST /api/customers

Create a new customer.

**Request Body**:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.org",
  "storeId": 1,
  "addressId": 5
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| firstName | string | Yes | Non-empty, max 45 chars |
| lastName | string | Yes | Non-empty, max 45 chars |
| email | string? | No | Valid email format, max 50 chars |
| storeId | int | Yes | Must reference existing store |
| addressId | int | Yes | Must reference existing address |

**Response 201** (Location header: `/api/customers/{id}`):
```json
{
  "id": 600,
  "storeId": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.org",
  "addressId": 5,
  "isActive": true,
  "createDate": "2026-02-22",
  "lastUpdate": "2026-02-22T10:30:00.000"
}
```

**Response 400** (validation errors):
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "firstName": ["'First Name' must not be empty."],
    "email": ["'Email' is not a valid email address."],
    "storeId": ["Store with ID 999 does not exist."]
  }
}
```

---

## PUT /api/customers/{id}

Full replacement update of an active customer.

**Path Parameters**:

| Parameter | Type | Description |
|-----------|------|-------------|
| id | int | Customer identifier |

**Request Body**: Same schema as POST.

**Response 200**:
```json
{
  "id": 1,
  "storeId": 2,
  "firstName": "Mary",
  "lastName": "Johnson",
  "email": "mary.johnson@example.org",
  "addressId": 10,
  "isActive": true,
  "createDate": "2006-02-14",
  "lastUpdate": "2026-02-22T11:00:00.000"
}
```

**Response 400**: Same as POST validation errors.

**Response 404** (not found or deactivated): Same as GET by ID.

---

## DELETE /api/customers/{id}

Soft-delete (deactivate) an active customer.

**Path Parameters**:

| Parameter | Type | Description |
|-----------|------|-------------|
| id | int | Customer identifier |

**Response 204**: No content (success).

**Response 404** (not found or already deactivated): Same as GET by ID.
