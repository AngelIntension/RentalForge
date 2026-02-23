# Feature Specification: Rental CRUD API

**Feature Branch**: `007-rental-crud`
**Created**: 2026-02-23
**Status**: Draft
**Input**: User description: "Implement full RESTful CRUD for the Rental entity as Feature #007. Controller-based routing (RentalsController), service layer with Ardalis.Result, aggregated validation, flat immutable DTOs, full TDD. POST accepts filmId + storeId and resolves available inventory. PUT return endpoint. DELETE admin hard delete. No payments, no bulk, no explicit inventory management."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Rent a Film (Priority: P1)

A staff member needs to create a new rental for a customer by specifying the film and store. The system automatically finds an available copy (inventory record) at the requested store and creates the rental. The staff member does not need to know or look up specific inventory identifiers — the system handles copy selection transparently.

**Why this priority**: Creating rentals is the primary business transaction of the entire rental system. Without this capability, no other rental operations are meaningful.

**Independent Test**: Can be fully tested by submitting a rental creation request with a film, store, customer, and staff member, then verifying the rental is created with a resolved inventory copy and the response includes all expected rental details.

**Acceptance Scenarios**:

1. **Given** a film has available copies at a store (at least one inventory record with no active rental), **When** a staff member creates a rental for that film and store with a valid customer and staff member, **Then** the system creates the rental, assigns one available inventory copy, and returns the rental details including the resolved inventory identifier, film title, customer name, and staff name.
2. **Given** a film has no available copies at a store (all inventory copies have active rentals), **When** a staff member attempts to create a rental, **Then** the system rejects the request with a clear message indicating no copies are available at that store.
3. **Given** a film has no inventory records at the requested store, **When** a staff member attempts to create a rental, **Then** the system rejects the request indicating the film is not stocked at that store.
4. **Given** the specified customer does not exist or is inactive, **When** a staff member attempts to create a rental, **Then** the system rejects the request with a validation error identifying the customer issue.
5. **Given** the specified staff member does not exist or is inactive, **When** a staff member attempts to create a rental, **Then** the system rejects the request with a validation error identifying the staff issue.
6. **Given** the specified film does not exist, **When** a staff member attempts to create a rental, **Then** the system rejects the request indicating the film does not exist.
7. **Given** multiple validation errors exist in the request (e.g., invalid customer and invalid film), **When** a staff member submits the request, **Then** the system returns all errors in a single response (aggregated, not early-return).

---

### User Story 2 - List and Filter Rentals (Priority: P1)

A staff member needs to browse rentals, filtering by customer and by active/returned status, to manage ongoing and past transactions. Results are paginated for manageable response sizes.

**Why this priority**: Viewing rental history is essential for day-to-day operations — checking what a customer currently has rented, finding overdue items, and reviewing transaction history.

**Independent Test**: Can be fully tested by sending list requests with various filter combinations and verifying the response includes correctly filtered, paginated results.

**Acceptance Scenarios**:

1. **Given** the system has rental records, **When** a user requests the rental list without filters, **Then** the system returns a paginated list of rentals with default page size. Each list item includes core rental fields (identifier, rental date, return date, inventory identifier, customer identifier, staff identifier, last update timestamp).
2. **Given** the system has rental records for multiple customers, **When** a user filters by a specific customer identifier, **Then** the system returns only rentals belonging to that customer.
3. **Given** the system has both active (not yet returned) and returned rentals, **When** a user filters with activeOnly=true, **Then** the system returns only rentals where the return date is not set.
4. **Given** the system has rental records, **When** a user filters with activeOnly=false or omits the parameter, **Then** the system returns all rentals (both active and returned).
5. **Given** the system has rental records, **When** a user combines a customer filter with activeOnly=true, **Then** the system returns only active rentals for that specific customer.
6. **Given** the system has rental records, **When** a user requests page 2 with a page size of 10, **Then** the system returns the second page of results with up to 10 records and includes pagination metadata (total count, total pages, current page).
7. **Given** no rentals match the filter criteria, **When** a user performs a filtered request, **Then** the system returns an empty list with zero total count.

---

### User Story 3 - View Rental Details (Priority: P1)

A staff member needs to view the full details of a specific rental, including the customer's name, the film title, the store, and the staff member who processed it, to answer customer inquiries or resolve disputes.

**Why this priority**: Viewing individual rental details with related display information is a core lookup operation — equally critical as listing.

**Independent Test**: Can be fully tested by requesting a specific rental by identifier and verifying all expected fields are returned, including flat display fields for related entities.

**Acceptance Scenarios**:

1. **Given** a rental exists in the system, **When** a user requests that rental's details, **Then** the system returns the rental's full information including: rental identifier, rental date, return date (if returned), inventory identifier, film identifier, film title, store identifier, customer identifier, customer first and last name, staff identifier, staff first and last name, and last update timestamp.
2. **Given** no rental exists with the requested identifier, **When** a user requests that rental's details, **Then** the system returns a "not found" response.

---

### User Story 4 - Return a Rental (Priority: P2)

A staff member needs to process a rental return when a customer brings back a film. This sets the return date on the rental, making the inventory copy available for future rentals.

**Why this priority**: Processing returns is essential to the rental lifecycle but depends on rentals existing first.

**Independent Test**: Can be fully tested by marking an active rental as returned and verifying the return date is set and the rental detail reflects the change.

**Acceptance Scenarios**:

1. **Given** an active rental (no return date set), **When** a staff member processes the return, **Then** the system sets the return date to the current timestamp and returns the updated rental details.
2. **Given** a rental that has already been returned, **When** a staff member attempts to return it again, **Then** the system rejects the request with an error indicating the rental has already been returned.
3. **Given** no rental exists with the requested identifier, **When** a staff member attempts to process a return, **Then** the system returns a "not found" response.

---

### User Story 5 - Delete a Rental (Priority: P3)

An administrator needs to permanently remove a rental record for administrative cleanup (e.g., correcting erroneous entries). This is a hard delete — the record is permanently removed.

**Why this priority**: Deletion is the least frequent operation and is typically an administrative cleanup task.

**Independent Test**: Can be fully tested by deleting a rental and verifying it no longer appears in list results or can be retrieved by identifier.

**Acceptance Scenarios**:

1. **Given** an existing rental with no associated payment records, **When** an administrator deletes the rental, **Then** the system permanently removes the rental record and returns a success confirmation.
2. **Given** an existing rental with associated payment records, **When** an administrator attempts to delete, **Then** the system returns a conflict error indicating the rental has dependent payment records.
3. **Given** no rental exists with the requested identifier, **When** an administrator attempts to delete, **Then** the system returns a "not found" response.
4. **Given** a rental has been deleted, **When** any user searches for or requests that rental, **Then** the rental does not appear in results and the detail request returns "not found".

---

### Edge Cases

- What happens when a film has multiple available copies at a store? The system selects one available copy deterministically (e.g., lowest inventory identifier) and assigns it to the rental.
- What happens when page number exceeds available pages? The system returns an empty list with the correct total count.
- What happens when page size is zero or negative? The system rejects the request with a validation error.
- What happens when page size exceeds the maximum allowed? The system caps it at the maximum (100) without error.
- What happens when a customerId filter references a non-existent customer? The system returns an empty list (no error — filtering by a non-existent ID simply yields no matches).
- What happens when a rental is created and then the same film+store combination is requested again? The previously assigned copy is no longer available; the system picks the next available copy or reports no availability.
- What happens when filmId, storeId, customerId, or staffId is zero or negative in the create request? The system rejects with validation errors for each invalid field.
- What happens when a staff member tries to return a rental that was already returned? The system rejects with a clear error indicating the rental has already been returned.
- What happens when concurrent requests attempt to rent the last available copy? One request succeeds and the other receives a "no available copies" error (database constraints ensure consistency).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a paginated list of rentals, defaulting to page 1 with a page size of 10. Each list item MUST include core rental fields (identifier, rental date, return date, inventory identifier, customer identifier, staff identifier, last update timestamp) using IDs only for related entities — no embedded names.
- **FR-002**: System MUST support filtering rentals by customer identifier using a `customerId` query parameter (exact match).
- **FR-003**: System MUST support filtering rentals by active status using an `activeOnly` boolean query parameter. When true, only rentals without a return date are returned. When false or omitted, all rentals are returned.
- **FR-004**: System MUST apply all provided filters using AND logic — a rental must satisfy every active filter to appear in results.
- **FR-005**: System MUST return pagination metadata with list results: total record count, total pages, current page number, and page size.
- **FR-006**: System MUST return the full details of a single rental by its unique identifier, including: rental identifier, rental date, return date, inventory identifier, film identifier, film title (flat), store identifier, customer identifier, customer first name (flat), customer last name (flat), staff identifier, staff first name (flat), staff last name (flat), and last update timestamp.
- **FR-007**: System MUST return a "not found" response when requesting, returning, or deleting a rental that does not exist.
- **FR-008**: System MUST allow creation of a new rental by accepting a film identifier, store identifier, customer identifier, and staff identifier. The system MUST automatically resolve one available inventory copy (an inventory record matching the film and store that has no active rental) and assign it to the new rental.
- **FR-009**: When no available inventory copy exists for the requested film at the requested store, the system MUST reject the rental creation with a clear, specific error message distinguishing between "film not stocked at this store" (no inventory records) and "all copies currently rented out" (inventory exists but all copies have active rentals).
- **FR-010**: System MUST validate all input on rental creation: film identifier must reference an existing film; store identifier must reference an existing store; customer identifier must reference an existing, active customer; staff identifier must reference an existing, active staff member. All four fields are required.
- **FR-011**: System MUST aggregate all validation and business-rule errors before responding — never early-return on the first failure.
- **FR-012**: System MUST allow processing a rental return, which sets the return date to the current timestamp. Returning an already-returned rental MUST be rejected with a clear error message.
- **FR-013**: System MUST implement hard delete for rentals — deletion permanently removes the rental record. Deletion MUST be blocked if the rental has associated payment records; the system returns a meaningful conflict error in that case.
- **FR-014**: System MUST set the rental date to the current timestamp when creating a new rental.
- **FR-015**: System MUST automatically update the last-updated timestamp on every modification.
- **FR-016**: System MUST return structured validation error responses that identify each invalid field and the reason for rejection.
- **FR-017**: System MUST never expose internal entity representations in responses — only dedicated response models are returned.
- **FR-018**: System MUST cap the maximum page size at 100 to prevent excessively large responses.
- **FR-019**: System MUST order rental list results by rental date descending by default (most recent first).
- **FR-020**: Response models MUST return identifiers (IDs) for related entities in list responses. Detail responses MUST include flat, inlined display fields (customer name, film title, staff name) alongside identifiers — not nested objects.

### Key Entities

- **Rental**: The central entity representing a film rental transaction. Key attributes: unique identifier, rental date, return date (empty until returned), inventory copy reference, customer reference, staff reference, last update timestamp. A rental is "active" when its return date is not set.
- **Inventory**: A physical copy of a film held at a specific store. Key attributes: unique identifier, film reference, store reference. The system resolves which inventory copy to assign during rental creation — this is an internal mechanism, not exposed to the user in the creation request.
- **Customer**: The person renting the film. Must be active (not deactivated) to create a new rental.
- **Staff**: The employee processing the rental. Must be active to be assigned to a new rental.
- **Film**: The title being rented. Referenced by identifier in the creation request; the system uses it to find available inventory at the specified store.
- **Store**: The physical location where the rental takes place. Referenced by identifier in the creation request.

### Assumptions

- The creation request accepts a film identifier and store identifier (not an inventory identifier). The system resolves to one available inventory copy transparently. This simplifies the rental workflow — staff do not need to know or manage inventory identifiers.
- When multiple inventory copies are available, the system selects one deterministically (e.g., lowest identifier). The specific selection strategy is an implementation detail.
- "Active rental" is defined as a rental whose return date has not been set.
- An inventory copy is "available" if it has no active rental (all of its rentals have return dates set, or it has no rentals at all).
- Rentals are ordered by rental date descending (most recent first) in list results, as this is the most useful default for operational staff.
- The `activeOnly` parameter defaults to false (show all rentals) when not provided.
- The delete operation is intended for administrators only. Authorization enforcement is out of scope for this feature and will be addressed when authentication is implemented. The endpoint is functional but unprotected until then.
- Payment CRUD is out of scope for this feature. However, existing payment records in the database are respected — rentals with associated payments cannot be deleted (referential integrity protection).
- No bulk operations (e.g., batch returns) are included — single-rental operations only per YAGNI.
- No explicit inventory management endpoints are included. Inventory is only interacted with indirectly through the rental creation flow.
- The rental date is set automatically by the system at creation time — it is not user-provided.
- The return date is set automatically by the system at return time — it is not user-provided.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Staff can create a new rental by specifying only a film, store, customer, and staff member — no inventory lookup required — and receive confirmation with full rental details in a single request.
- **SC-002**: Staff receive a clear, specific error message when attempting to rent a film with no available copies, distinguishing between "not stocked" and "all copies rented."
- **SC-003**: Staff can retrieve a paginated rental list, filtered by customer and/or active status, in under 1 second for up to 50,000 rental records.
- **SC-004**: Staff can view any individual rental's full details — including customer name, film title, and staff name — in a single request.
- **SC-005**: Staff can process a rental return in a single request and the film copy becomes immediately available for new rentals.
- **SC-006**: Administrators can permanently remove a rental record, and the rental no longer appears in any list or detail requests.
- **SC-007**: All invalid inputs are rejected with clear, field-specific error messages — all errors aggregated in a single response.
- **SC-008**: Every acceptance scenario has at least one automated test that verifies the expected behavior end-to-end.
