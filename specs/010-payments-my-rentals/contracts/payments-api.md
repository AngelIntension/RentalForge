# API Contract: Payments

**Feature Branch**: `010-payments-my-rentals`
**Date**: 2026-02-25

## POST /api/payments

**Auth**: `[Authorize(Roles = "Staff,Admin")]`

**Request body**:
```json
{
  "rentalId": 1001,
  "amount": 4.99,
  "paymentDate": "2026-02-25T14:30:00Z",
  "staffId": 1
}
```

| Field       | Type    | Required | Notes                          |
| ----------- | ------- | -------- | ------------------------------ |
| rentalId    | int     | Yes      | Must reference existing rental |
| amount      | decimal | Yes      | Must be > 0, max 999.99       |
| paymentDate | string  | No       | ISO 8601, defaults to UTC now  |
| staffId     | int     | Yes      | Must reference active staff    |

**Success (201 Created)**:
```json
{
  "id": 5001,
  "rentalId": 1001,
  "customerId": 42,
  "customerFirstName": "Jane",
  "customerLastName": "Doe",
  "staffId": 1,
  "staffFirstName": "Mike",
  "staffLastName": "Hillyer",
  "amount": 4.99,
  "paymentDate": "2026-02-25T14:30:00Z",
  "filmTitle": "Academy Dinosaur"
}
```

**Location header**: `/api/payments/5001`

**Error (400 Validation)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "amount": ["'Amount' must be greater than '0'."],
    "rentalId": ["Rental with ID 9999 was not found."]
  }
}
```

**Note**: Rental-not-found is returned as a 400 validation error (`"rentalId": ["Rental with ID 9999 was not found."]`), aggregated with any other validation errors per the aggregate-all-errors pattern. A standalone 404 is not used for this endpoint.

---

## GET /api/payments

**Auth**: `[Authorize]` — role-based scoping applied server-side.

**Query parameters**:

| Param      | Type | Required | Notes                                          |
| ---------- | ---- | -------- | ---------------------------------------------- |
| customerId | int  | No       | Filter by customer (forced for Customer role)  |
| staffId    | int  | No       | Filter by processing staff                     |
| rentalId   | int  | No       | Filter by rental                               |
| page       | int  | No       | Default: 1                                     |
| pageSize   | int  | No       | Default: 10                                    |

**Role-based behavior**:
- **Admin**: No restrictions. All filters honored.
- **Staff**: Results scoped to payments for rentals at the staff member's store (via Rental → Inventory → Store). Filters further narrow within that scope.
- **Customer**: Results scoped to own customerId. Any client-provided customerId filter is ignored/overridden.

**Success (200)**:
```json
{
  "items": [
    {
      "id": 5001,
      "rentalId": 1001,
      "customerId": 42,
      "staffId": 1,
      "amount": 4.99,
      "paymentDate": "2026-02-25T14:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

---

## PUT /api/rentals/{id}/return (enhanced)

**Auth**: `[Authorize(Roles = "Staff,Admin")]` (unchanged)

**Request body** (optional — omit entirely for backward-compatible behavior):
```json
{
  "amount": 4.99,
  "staffId": 1
}
```

| Field   | Type    | Required | Notes                                |
| ------- | ------- | -------- | ------------------------------------ |
| amount  | decimal | No       | If provided, must be > 0            |
| staffId | int     | No       | Required when amount is provided     |

**Behavior**:
- No body or `null` body → Return rental only (backward compatible, identical to current behavior)
- Body with `amount` → Return rental AND create payment in single operation
- Body with invalid `amount` (≤ 0) → Reject with 400, no return processed

**Success (200)**: Returns enhanced `RentalDetailResponse` (same as GET /api/rentals/{id}), now including payment summary fields.

**Error (400)**: Validation errors (already returned, amount ≤ 0, missing staffId when amount provided).
**Error (404)**: Rental not found.
