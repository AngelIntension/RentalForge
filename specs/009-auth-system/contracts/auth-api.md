# API Contract: Authentication Endpoints

**Base path**: `/api/auth`
**Controller**: `AuthController`

---

## POST /api/auth/register

Register a new user account.

**Auth**: `[AllowAnonymous]` (public registration defaults to Customer role). Authenticated Admin can assign elevated roles.

**Request body**:
```json
{
  "email": "user@example.com",
  "password": "SecureP@ss1",
  "role": "Customer"
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| email | string | Yes | Valid email format, unique |
| password | string | Yes | Min 8 chars, 1 upper, 1 lower, 1 digit, 1 special |
| role | string? | No | Default: "Customer". "Staff"/"Admin" require authenticated Admin. |

**Success response**: `201 Created`
```json
{
  "token": "eyJhbGciOi...",
  "refreshToken": "dGhpcyBpcyBh...",
  "user": {
    "id": "guid-string",
    "email": "user@example.com",
    "role": "Customer",
    "customerId": null,
    "createdAt": "2026-02-24T10:00:00Z"
  }
}
```

**Error responses**:

| Status | Condition |
|--------|-----------|
| 400 | Validation errors (aggregated — email format, password strength, duplicate email) |
| 403 | Non-Admin attempting to register with elevated role |
| 429 | Rate limit exceeded (3 per minute) |

400 body follows `ValidationProblemDetails`:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "email": ["Email is already registered."],
    "password": [
      "Password must be at least 8 characters.",
      "Password must contain at least one uppercase letter."
    ]
  }
}
```

---

## POST /api/auth/login

Authenticate with email and password.

**Auth**: `[AllowAnonymous]`

**Request body**:
```json
{
  "email": "user@example.com",
  "password": "SecureP@ss1"
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| email | string | Yes | Non-empty, valid email format |
| password | string | Yes | Non-empty |

**Success response**: `200 OK`
```json
{
  "token": "eyJhbGciOi...",
  "refreshToken": "dGhpcyBpcyBh...",
  "user": {
    "id": "guid-string",
    "email": "user@example.com",
    "role": "Staff",
    "customerId": null,
    "createdAt": "2026-02-24T10:00:00Z"
  }
}
```

**Error responses**:

| Status | Condition |
|--------|-----------|
| 400 | Validation errors (empty fields) |
| 401 | Invalid credentials (generic message: "Invalid email or password.") |
| 429 | Rate limit exceeded (5 per minute) |

401 body:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Invalid email or password.",
  "status": 401
}
```

---

## POST /api/auth/refresh

Exchange a valid refresh token for new access + refresh tokens (rotation).

**Auth**: `[AllowAnonymous]`

**Request body**:
```json
{
  "refreshToken": "dGhpcyBpcyBh..."
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| refreshToken | string | Yes | Non-empty |

**Success response**: `200 OK`
```json
{
  "token": "eyJhbGciOi...(new access token)",
  "refreshToken": "bmV3IHJlZnJl...(new refresh token)"
}
```

**Error responses**:

| Status | Condition |
|--------|-----------|
| 400 | Missing refresh token |
| 401 | Invalid, expired, consumed, or revoked refresh token |
| 401 | Reuse detected — entire family revoked |
| 429 | Rate limit exceeded (10 per minute) |

---

## POST /api/auth/logout

Invalidate the current refresh token (and its family).

**Auth**: `[Authorize]` (any authenticated user)

**Request body**:
```json
{
  "refreshToken": "dGhpcyBpcyBh..."
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| refreshToken | string | Yes | Non-empty |

**Success response**: `204 No Content`

**Error responses**:

| Status | Condition |
|--------|-----------|
| 401 | Unauthenticated |

**Note**: Logout always returns 204 even if the refresh token is already invalid/expired (idempotent).

---

## GET /api/auth/me

Get the current authenticated user's profile.

**Auth**: `[Authorize]` (any authenticated user)

**Request body**: None

**Success response**: `200 OK`
```json
{
  "id": "guid-string",
  "email": "user@example.com",
  "role": "Customer",
  "customerId": 42,
  "createdAt": "2026-02-24T10:00:00Z"
}
```

**Error responses**:

| Status | Condition |
|--------|-----------|
| 401 | Unauthenticated |

---

## Shared DTOs

### AuthResponse (login + register)

```
token:        string   — JWT access token
refreshToken: string   — Opaque refresh token
user:         UserDto  — Authenticated user info
```

### RefreshResponse (refresh)

```
token:        string   — New JWT access token
refreshToken: string   — New opaque refresh token
```

### UserDto (me + embedded in auth responses)

```
id:           string   — User GUID
email:        string   — User email
role:         string   — "Admin" | "Staff" | "Customer"
customerId:   int?     — Linked dvdrental Customer ID (null if not linked)
createdAt:    DateTime — Account creation timestamp (UTC)
```

---

## Rate Limiting Summary

| Endpoint | Window | Limit | Policy Name |
|----------|--------|-------|-------------|
| POST /api/auth/login | Fixed 1 min | 5 | `auth-login` |
| POST /api/auth/register | Fixed 1 min | 3 | `auth-register` |
| POST /api/auth/refresh | Fixed 1 min | 10 | `auth-refresh` |

Rate limit responses return `429 Too Many Requests` with `Retry-After` header.

---

## JWT Claims

| Claim | Type | Value |
|-------|------|-------|
| `sub` | string | User ID (GUID) |
| `email` | string | User email |
| `role` | string | "Admin", "Staff", or "Customer" |
| `iat` | number | Issued-at timestamp |
| `exp` | number | Expiry timestamp (15 minutes from issue) |
| `jti` | string | Unique token identifier (GUID) |

---

## Authorization Matrix (existing + new endpoints)

| Endpoint | Anonymous | Customer | Staff | Admin |
|----------|-----------|----------|-------|-------|
| GET /health | Yes | Yes | Yes | Yes |
| POST /api/auth/register | Yes (Customer only) | Yes (Customer only) | Yes (Customer only) | Yes (any role) |
| POST /api/auth/login | Yes | - | - | - |
| POST /api/auth/refresh | Yes | - | - | - |
| POST /api/auth/logout | No | Yes | Yes | Yes |
| GET /api/auth/me | No | Yes | Yes | Yes |
| GET /api/customers | No | No | Yes | Yes |
| GET /api/customers/{id} | No | Yes (own only) | Yes | Yes |
| POST /api/customers | No | No | Yes | Yes |
| PUT /api/customers/{id} | No | No | Yes | Yes |
| DELETE /api/customers/{id} | No | No | Yes | Yes |
| GET /api/films | No | Yes (read) | Yes | Yes |
| GET /api/films/{id} | No | Yes (read) | Yes | Yes |
| POST /api/films | No | No | Yes | Yes |
| PUT /api/films/{id} | No | No | Yes | Yes |
| DELETE /api/films/{id} | No | No | Yes | Yes |
| GET /api/rentals | No | Yes (own only) | Yes | Yes |
| GET /api/rentals/{id} | No | Yes (own only) | Yes | Yes |
| POST /api/rentals | No | No | Yes | Yes |
| PUT /api/rentals/{id}/return | No | No | Yes | Yes |
| DELETE /api/rentals/{id} | No | No | Yes | Yes |
