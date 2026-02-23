# Research: Rental CRUD API

**Branch**: `007-rental-crud` | **Date**: 2026-02-23

## Research Topics & Decisions

### R-001: Inventory Resolution Strategy

**Decision**: Service layer queries for available inventory using a single
EF Core query: find inventory records matching filmId + storeId where
none of the inventory's rentals have a null ReturnDate. Select the first
match ordered by InventoryId (deterministic, lowest ID).

**Rationale**: This translates to efficient SQL with a `NOT EXISTS`
subquery or `LEFT JOIN` anti-pattern. EF Core handles the translation
cleanly. Deterministic ordering ensures predictable behavior for tests.
The dvdrental sample data has ~4,500 inventory records spread across
2 stores, so no performance concern.

**Alternatives considered**:
- Random selection from available copies: Non-deterministic, harder to test,
  and provides no user benefit. Rejected.
- Stored procedure for resolution: Over-engineered. EF Core query is
  sufficient for this scale and keeps logic in the service layer. Rejected.
- Optimistic concurrency with retry: The database's `ON CONFLICT` or
  unique constraint would handle true race conditions. For now, a simple
  check-then-insert is sufficient — the unique index on
  `(rental_date, inventory_id, customer_id)` provides a safety net.
  Explicit retry logic is YAGNI. Rejected.

### R-002: Return Endpoint Design

**Decision**: Implement as `PUT /api/rentals/{id}/return` — a sub-resource
action endpoint that sets ReturnDate to the current UTC timestamp. No
request body required. Returns the updated `RentalDetailResponse`.

**Rationale**: A return is semantically distinct from a general update.
Using a dedicated sub-resource endpoint makes the API self-documenting
and prevents accidental field overwrites. The PUT verb is appropriate
because the operation is idempotent in intent (returning an already-returned
rental is rejected, not silently repeated).

**Alternatives considered**:
- PATCH /api/rentals/{id} with `{ "returnDate": "..." }`: Allows
  arbitrary return dates, which is a data integrity risk. Also requires
  a general PATCH infrastructure we don't need. Rejected.
- POST /api/rentals/{id}/return: POST would imply creating a new resource.
  PUT is more semantically correct for updating state. Rejected.
- PUT /api/rentals/{id} with full body: Would require sending all fields
  just to set the return date. Over-engineered for a single-field state
  transition. Rejected.

### R-003: List Response DTO (Lean) vs Detail Response DTO (Rich)

**Decision**: Two response types:
- `RentalListResponse`: Core rental fields + IDs only (rentalId, rentalDate,
  returnDate, inventoryId, customerId, staffId, lastUpdate). No embedded names.
- `RentalDetailResponse`: All list fields + filmId, filmTitle (flat),
  storeId, customerFirstName, customerLastName (flat), staffFirstName,
  staffLastName (flat).

**Rationale**: Constitution v1.9.0 mandates lean list DTOs (IDs for related
entities) and flat detail DTOs (inlined names). Separating the two keeps
list payloads small — critical for the ~16K rental records in dvdrental.
The detail DTO traverses Rental → Inventory → Film for filmId/filmTitle
and Rental → Inventory → Store for storeId.

**Alternatives considered**:
- Single DTO for both: Would either bloat list responses or starve detail
  responses. Rejected.
- Include film title in list: Would require a join to Inventory → Film on
  every list query. Rejected per lean list principle.

### R-004: Hard Delete with Payment Protection

**Decision**: Service layer checks for associated payment records via
`db.Payments.AnyAsync(p => p.RentalId == id)`. If payments exist, return
`Result.Conflict()`. Otherwise, delete the rental record.

**Rationale**: The dvdrental schema has `ON DELETE RESTRICT` on
`payment.rental_id`, so the DB would reject the delete anyway — but
checking first lets us return a clean 409 Conflict instead of an unhandled
DB exception. This follows the same pattern used in Film CRUD (inventory
blocks film deletion).

**Alternatives considered**:
- Let DB throw and catch the exception: Violates constitution Principle VI
  (exceptions for expected outcomes prohibited). Rejected.
- Cascade delete payments: Destroying financial records is a data integrity
  risk. Rejected.
- Soft delete rentals: The Rental entity has no `active` flag like Customer.
  Adding one would require schema changes beyond scope. Rejected per YAGNI.

### R-005: Create Request Validation

**Decision**: Use FluentValidation for basic field validation (all four IDs
must be > 0). Service layer handles FK existence checks (film exists,
store exists, customer exists + active, staff exists + active) and business
rule validation (inventory availability). All errors are aggregated into a
single `Result<T>.Invalid(allErrors)` response.

**Rationale**: Follows the established pattern from Customer and Film CRUD.
FluentValidation handles shape validation; service layer handles business
rules. The two error sources are merged via `validationResult.AsErrors()`
bridge before checking FK existence.

**Alternatives considered**:
- All validation in FluentValidation (including FK checks): Would require
  injecting DbContext into validators, violating separation of concerns.
  Rejected.
- No FluentValidation (all in service): Would lose the clean declarative
  validation syntax for basic field constraints. Rejected.

### R-006: Test Data Seeding for Rentals

**Decision**: Create a `RentalTestHelper` class (mirroring `CustomerTestHelper`
and `FilmTestHelper`) that seeds: stores, staff, customers, films, languages,
inventory records, and rental records. Uses raw SQL with
`session_replication_role = 'replica'` for FK cycle workarounds where needed.

**Rationale**: Rental tests need related data across 6+ tables (rental →
inventory → film/store, rental → customer, rental → staff, payment → rental).
A dedicated helper keeps test setup DRY and consistent. The helper needs
methods to create: available inventory (for create tests), active rentals
(for return tests), returned rentals (for re-return rejection tests), and
rentals with payments (for delete-blocking tests).

**Alternatives considered**:
- Inline seeding per test class: Too much duplication across 6+ related
  tables. Rejected.
- Shared database state: Violates test isolation principle. Rejected.
