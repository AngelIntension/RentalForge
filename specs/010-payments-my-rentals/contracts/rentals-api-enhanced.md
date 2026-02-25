# API Contract: Enhanced Rental Responses

**Feature Branch**: `010-payments-my-rentals`
**Date**: 2026-02-25

## GET /api/rentals (enhanced response)

Response items now include payment summary fields.

**Enhanced RentalListResponse**:
```json
{
  "items": [
    {
      "id": 1001,
      "rentalDate": "2026-02-20T10:00:00Z",
      "returnDate": null,
      "inventoryId": 500,
      "customerId": 42,
      "staffId": 1,
      "lastUpdate": "2026-02-20T10:00:00Z",
      "totalPaid": 4.99,
      "rentalRate": 4.99,
      "outstandingBalance": 0.00
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

New fields:

| Field              | Type    | Description                               |
| ------------------ | ------- | ----------------------------------------- |
| totalPaid          | decimal | Sum of all payment amounts for the rental |
| rentalRate         | decimal | Film's rental rate                        |
| outstandingBalance | decimal | rentalRate - totalPaid (can be negative)  |

---

## GET /api/rentals/{id} (enhanced response)

Now includes payment summary and per-rental payment history.

**Enhanced RentalDetailResponse**:
```json
{
  "id": 1001,
  "rentalDate": "2026-02-20T10:00:00Z",
  "returnDate": "2026-02-25T14:30:00Z",
  "inventoryId": 500,
  "filmId": 100,
  "filmTitle": "Academy Dinosaur",
  "storeId": 1,
  "customerId": 42,
  "customerFirstName": "Jane",
  "customerLastName": "Doe",
  "staffId": 1,
  "staffFirstName": "Mike",
  "staffLastName": "Hillyer",
  "lastUpdate": "2026-02-25T14:30:00Z",
  "totalPaid": 4.99,
  "rentalRate": 4.99,
  "outstandingBalance": 0.00,
  "payments": [
    {
      "id": 5001,
      "amount": 4.99,
      "paymentDate": "2026-02-25T14:30:00Z",
      "staffId": 1
    }
  ]
}
```

New fields:

| Field              | Type                | Description                               |
| ------------------ | ------------------- | ----------------------------------------- |
| totalPaid          | decimal             | Sum of all payment amounts                |
| rentalRate         | decimal             | Film's rental rate                        |
| outstandingBalance | decimal             | rentalRate - totalPaid                    |
| payments           | RentalPaymentItem[] | All payments for this rental, chronological |

**RentalPaymentItem**:

| Field       | Type     | Description                |
| ----------- | -------- | -------------------------- |
| id          | int      | Payment ID                 |
| amount      | decimal  | Payment amount             |
| paymentDate | string   | ISO 8601 payment timestamp |
| staffId     | int      | Staff who processed it     |
