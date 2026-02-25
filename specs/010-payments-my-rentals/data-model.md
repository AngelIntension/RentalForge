# Data Model: Payments & My Rentals Enhancement

**Feature Branch**: `010-payments-my-rentals`
**Date**: 2026-02-25

## Entities

### Payment (existing — no changes)

| Field       | Type          | DB Column      | Constraints                          |
| ----------- | ------------- | -------------- | ------------------------------------ |
| PaymentId   | int           | payment_id     | PK, auto-increment                   |
| CustomerId  | int           | customer_id    | FK → customer (smallint), NOT NULL   |
| StaffId     | int           | staff_id       | FK → staff (smallint), NOT NULL      |
| RentalId    | int           | rental_id      | FK → rental, NOT NULL                |
| Amount      | decimal       | amount         | numeric(5,2), NOT NULL, must be > 0  |
| PaymentDate | DateTime      | payment_date   | NOT NULL, defaults to UTC now        |

**Relationships**:
- Many-to-One → Customer (a customer has many payments)
- Many-to-One → Staff (a staff member processes many payments)
- Many-to-One → Rental (a rental has many payments)

**Business Rules**:
- Amount must be positive (> 0)
- CustomerId is derived from the Rental's CustomerId (FR-004)
- StaffId must reference an active staff member
- RentalId must reference an existing rental
- PaymentDate defaults to DateTime.UtcNow when not provided

### ApplicationUser (existing — add StaffId)

| Field    | Type | DB Column | Constraints                         |
| -------- | ---- | --------- | ----------------------------------- |
| StaffId  | int? | StaffId   | FK → staff (smallint), nullable     |

**Rationale**: Mirrors the existing `CustomerId` nullable FK pattern. Provides a reliable FK-based link from Identity users to dvdrental Staff records, avoiding fragile email-matching. Used by `GetCurrentUserStoreId()` to resolve the staff member's store for role-based payment scoping.

**Migration**: Single `AddColumn` migration — adds nullable `StaffId` column + FK constraint + index to `AspNetUsers` table.

**Seeder update**: `DevDataSeeder.SeedAuthUsersAsync()` sets `StaffId` on staff/admin users (Staff user → staff ID 1, Admin user → staff ID 1 or separate staff record as applicable).

### Rental (existing — enhanced responses only)

No entity changes. The response DTOs are enhanced with computed payment summary fields.

**Computed fields (in DTOs, not entity)**:
- `TotalPaid`: Sum of all Payment.Amount for this rental
- `RentalRate`: From Inventory.Film.RentalRate
- `OutstandingBalance`: RentalRate - TotalPaid (can be negative for overpayment)

### Relationships Diagram

```
Film (RentalRate)
  └── Inventory (StoreId)
        └── Rental (CustomerId, StaffId)
              └── Payment (CustomerId, StaffId, Amount, PaymentDate)
```

Store-scoping path: Payment → Rental → Inventory → Store

## DTOs

### PaymentListResponse (lean, IDs only)

| Field       | Type     | Source           |
| ----------- | -------- | ---------------- |
| Id          | int      | Payment.PaymentId |
| RentalId    | int      | Payment.RentalId  |
| CustomerId  | int      | Payment.CustomerId |
| StaffId     | int      | Payment.StaffId   |
| Amount      | decimal  | Payment.Amount    |
| PaymentDate | DateTime | Payment.PaymentDate |

### PaymentDetailResponse (flat with denormalized names)

| Field              | Type     | Source                      |
| ------------------ | -------- | --------------------------- |
| Id                 | int      | Payment.PaymentId           |
| RentalId           | int      | Payment.RentalId            |
| CustomerId         | int      | Payment.CustomerId          |
| CustomerFirstName  | string   | Payment.Customer.FirstName  |
| CustomerLastName   | string   | Payment.Customer.LastName   |
| StaffId            | int      | Payment.StaffId             |
| StaffFirstName     | string   | Payment.Staff.FirstName     |
| StaffLastName      | string   | Payment.Staff.LastName      |
| Amount             | decimal  | Payment.Amount              |
| PaymentDate        | DateTime | Payment.PaymentDate         |
| FilmTitle          | string   | Rental.Inventory.Film.Title |

### CreatePaymentRequest

| Field       | Type      | Validation                        |
| ----------- | --------- | --------------------------------- |
| RentalId    | int       | > 0, must reference existing rental |
| Amount      | decimal   | > 0                               |
| PaymentDate | DateTime? | Optional, defaults to UtcNow      |
| StaffId     | int       | > 0, must reference active staff  |

### ReturnRentalRequest (new — optional body for enhanced return)

| Field   | Type     | Validation                   |
| ------- | -------- | ---------------------------- |
| Amount  | decimal? | If provided, must be > 0     |
| StaffId | int?     | If Amount provided, required and > 0 |

### Enhanced RentalListResponse (adds payment summary)

Existing fields plus:

| Field              | Type    | Source                                     |
| ------------------ | ------- | ------------------------------------------ |
| TotalPaid          | decimal | Sum(Payments.Amount)                       |
| RentalRate         | decimal | Inventory.Film.RentalRate                  |
| OutstandingBalance | decimal | RentalRate - TotalPaid                     |

### Enhanced RentalDetailResponse (adds payment history)

Existing fields plus:

| Field              | Type                  | Source                        |
| ------------------ | --------------------- | ----------------------------- |
| TotalPaid          | decimal               | Sum(Payments.Amount)          |
| RentalRate         | decimal               | Inventory.Film.RentalRate     |
| OutstandingBalance | decimal               | RentalRate - TotalPaid        |
| Payments           | RentalPaymentItem[]   | Rental.Payments (inline list) |

### RentalPaymentItem (inline in RentalDetailResponse)

| Field       | Type     | Source              |
| ----------- | -------- | ------------------- |
| Id          | int      | Payment.PaymentId   |
| Amount      | decimal  | Payment.Amount      |
| PaymentDate | DateTime | Payment.PaymentDate |
| StaffId     | int      | Payment.StaffId     |

## Validation Rules Summary

### CreatePaymentRequest
1. RentalId > 0 (FluentValidation)
2. Amount > 0 (FluentValidation)
3. StaffId > 0 (FluentValidation)
4. Rental must exist (service-level)
5. Staff must exist and be active (service-level)
6. Payment customer must match rental customer (service-level, derived from rental)
7. All errors aggregated before response

### ReturnRentalRequest (when provided)
1. Amount > 0 if provided (FluentValidation)
2. StaffId > 0 and required if Amount provided (FluentValidation)
3. Staff must exist and be active if payment included (service-level)
4. All errors aggregated with existing return validation
