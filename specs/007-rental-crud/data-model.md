# Data Model: Rental CRUD API

**Branch**: `007-rental-crud` | **Date**: 2026-02-23

## Existing Entities (read-only — no schema changes)

All entities below already exist in the `dvdrental` database and are
scaffolded as EF Core entity classes. No migrations needed.

### Rental (primary entity)

| Column | Type | Nullable | Constraints | Notes |
|--------|------|----------|-------------|-------|
| rental_id | int (serial) | NO | PK | Auto-generated |
| rental_date | timestamp | NO | | Set at creation time |
| inventory_id | int | NO | FK → inventory | Resolved from filmId + storeId |
| customer_id | smallint | NO | FK → customer | Must be active |
| return_date | timestamp | YES | | Null = active rental |
| staff_id | smallint | NO | FK → staff | Must be active |
| last_update | timestamp | NO | | Auto-set via DB default `now()` |

**Relationships:**
- `Rental → Inventory` (many-to-one) via `inventory_id`
- `Rental → Customer` (many-to-one) via `customer_id`
- `Rental → Staff` (many-to-one) via `staff_id`
- `Rental → Payment` (one-to-many) — blocks hard delete

**Unique index**: `(rental_date, inventory_id, customer_id)` — provides
natural race condition protection at the DB level.

### Inventory (resolution target)

| Column | Type | Nullable | Constraints | Notes |
|--------|------|----------|-------------|-------|
| inventory_id | int (serial) | NO | PK | Auto-generated |
| film_id | smallint | NO | FK → film | Film this copy represents |
| store_id | smallint | NO | FK → store | Store holding this copy |
| last_update | timestamp | NO | | |

**Relationships:**
- `Inventory → Film` (many-to-one) via `film_id`
- `Inventory → Store` (many-to-one) via `store_id`
- `Inventory → Rental` (one-to-many) — used to check availability

**Availability rule**: An inventory record is "available" if it has no
rental with `return_date IS NULL`.

### Customer (FK reference)

| Column | Type | Nullable | Constraints | Notes |
|--------|------|----------|-------------|-------|
| customer_id | int (serial) | NO | PK | |
| first_name | varchar(45) | NO | | Used in detail response |
| last_name | varchar(45) | NO | | Used in detail response |
| activebool | boolean | NO | | Must be true for new rentals |
| store_id | smallint | NO | FK → store | Customer's home store |

### Staff (FK reference)

| Column | Type | Nullable | Constraints | Notes |
|--------|------|----------|-------------|-------|
| staff_id | int (serial) | NO | PK | |
| first_name | varchar(45) | NO | | Used in detail response |
| last_name | varchar(45) | NO | | Used in detail response |
| active | boolean | NO | | Must be true for new rentals |
| store_id | smallint | NO | FK → store | Staff's assigned store |

### Film (FK reference via Inventory)

| Column | Type | Nullable | Constraints | Notes |
|--------|------|----------|-------------|-------|
| film_id | int (serial) | NO | PK | |
| title | varchar(255) | NO | | Used in detail response |

### Store (FK reference via Inventory)

| Column | Type | Nullable | Constraints | Notes |
|--------|------|----------|-------------|-------|
| store_id | int (serial) | NO | PK | |

### Payment (blocks deletion)

| Column | Type | Nullable | Constraints | Notes |
|--------|------|----------|-------------|-------|
| payment_id | int (serial) | NO | PK | |
| rental_id | int | NO | FK → rental (RESTRICT) | Blocks rental deletion |
| amount | numeric(5,2) | NO | | |

---

## DTO Models (new — to be created)

### RentalListResponse (lean list item)

```text
record RentalListResponse(
    int Id,
    DateTime RentalDate,
    DateTime? ReturnDate,
    int InventoryId,
    int CustomerId,
    int StaffId,
    DateTime LastUpdate
)
```

**Design rationale**: Constitution v1.9.0 — IDs only for related entities,
no embedded names. Keeps list payload small for ~16K rental records.

### RentalDetailResponse (rich detail)

```text
record RentalDetailResponse(
    int Id,
    DateTime RentalDate,
    DateTime? ReturnDate,
    int InventoryId,
    int FilmId,                  ← from Inventory → Film
    string FilmTitle,            ← flat, one-level relationship
    int StoreId,                 ← from Inventory → Store
    int CustomerId,
    string CustomerFirstName,    ← flat, one-level relationship
    string CustomerLastName,     ← flat, one-level relationship
    int StaffId,
    string StaffFirstName,       ← flat, one-level relationship
    string StaffLastName,        ← flat, one-level relationship
    DateTime LastUpdate
)
```

**Design rationale**: Constitution v1.9.0 — one-level relationships
inlined as flat properties. Film title, customer name, and staff name
are string properties alongside their IDs. Store and film accessed via
Inventory navigation (Rental → Inventory → Film, Rental → Inventory → Store).

### CreateRentalRequest

```text
record CreateRentalRequest {
    int FilmId        ← required, > 0, FK existence checked in service
    int StoreId       ← required, > 0, FK existence checked in service
    int CustomerId    ← required, > 0, FK existence + active checked in service
    int StaffId       ← required, > 0, FK existence + active checked in service
}
```

**Design rationale**: Accepts filmId + storeId instead of inventoryId.
The service layer resolves to an available inventory copy transparently.
All four fields are simple int IDs — no complex nested structures needed.

---

## Validation Rules

### FluentValidation (CreateRentalValidator)

| Field | Rule | Message |
|-------|------|---------|
| FilmId | GreaterThan(0) | Default FluentValidation message |
| StoreId | GreaterThan(0) | Default FluentValidation message |
| CustomerId | GreaterThan(0) | Default FluentValidation message |
| StaffId | GreaterThan(0) | Default FluentValidation message |

### Service-Layer Validation (FK existence + business rules)

| Check | Error |
|-------|-------|
| FilmId exists in films table | "Film with ID {id} does not exist." |
| StoreId exists in stores table | "Store with ID {id} does not exist." |
| CustomerId exists in customers table and is active | "Customer with ID {id} does not exist or is inactive." |
| StaffId exists in staff table and is active | "Staff member with ID {id} does not exist or is inactive." |
| Inventory exists for filmId + storeId | "Film '{title}' is not stocked at store {storeId}." |
| Available inventory copy exists (no active rental) | "All copies of film '{title}' at store {storeId} are currently rented out." |

### Service-Layer Validation (return endpoint)

| Check | Error |
|-------|-------|
| Rental exists | Result.NotFound() |
| Rental has no return date (is active) | "Rental with ID {id} has already been returned." |

### Service-Layer Validation (delete endpoint)

| Check | Error |
|-------|-------|
| Rental exists | Result.NotFound() |
| Rental has no associated payments | "Cannot delete rental with ID {id} because it has associated payment records." |

All FluentValidation errors and service-layer errors are aggregated into a
single `Result<T>.Invalid(allErrors)` response (create endpoint).

---

## State Transitions

Rental has a simple two-state lifecycle based on the `return_date` column.

| Trigger | Before | After |
|---------|--------|-------|
| POST /api/rentals | Not exists | Active (return_date = null, rental_date = now) |
| PUT /api/rentals/{id}/return | Active (return_date = null) | Returned (return_date = now) |
| PUT /api/rentals/{id}/return | Returned (return_date set) | Rejected — 400 "already returned" |
| DELETE /api/rentals/{id} | Exists (no payments) | Permanently removed |
| DELETE /api/rentals/{id} | Exists (has payments) | Unchanged — 409 Conflict |
