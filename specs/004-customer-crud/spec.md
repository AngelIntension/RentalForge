# Feature Specification: Customer CRUD API

**Feature Branch**: `004-customer-crud`
**Created**: 2026-02-22
**Status**: Draft
**Input**: User description: "Implement full RESTful CRUD + search/pagination for the Customer entity using the existing DvdrentalContext and scaffolded entities."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse and Search Customers (Priority: P1)

A staff member needs to browse the customer list and search for specific customers by name or email to quickly locate customer records for rental transactions, account inquiries, or support requests.

**Why this priority**: Browsing and searching is the most frequently performed operation — staff need to find customers before they can do anything else (view details, update records, process rentals).

**Independent Test**: Can be fully tested by sending search queries to the customer list endpoint and verifying that results are filtered, paginated, and returned in the expected format.

**Acceptance Scenarios**:

1. **Given** the system has customer records, **When** a staff member requests the customer list without filters, **Then** the system returns a paginated list of active customers with default page size.
2. **Given** the system has customer records, **When** a staff member searches by a partial first name, **Then** the system returns only customers whose first name contains the search term (case-insensitive).
3. **Given** the system has customer records, **When** a staff member searches by a partial last name, **Then** the system returns only customers whose last name contains the search term (case-insensitive).
4. **Given** the system has customer records, **When** a staff member searches by a partial email address, **Then** the system returns only customers whose email contains the search term (case-insensitive).
5. **Given** the system has customer records, **When** a staff member requests page 2 with a page size of 10, **Then** the system returns the second page of results with up to 10 records and includes pagination metadata (total count, total pages, current page).
6. **Given** no customers match the search term, **When** a staff member performs a search, **Then** the system returns an empty list with zero total count.

---

### User Story 2 - View Customer Details (Priority: P1)

A staff member needs to view the full details of a specific customer, including their associated store and address identifiers, to assist the customer or verify their account.

**Why this priority**: Viewing individual customer details is a core operation needed for any customer interaction — equally critical as search.

**Independent Test**: Can be fully tested by requesting a specific customer by identifier and verifying all expected fields are returned.

**Acceptance Scenarios**:

1. **Given** a customer exists in the system, **When** a staff member requests that customer's details, **Then** the system returns the customer's full information including name, email, store identifier, address identifier, and active status.
2. **Given** no customer exists with the requested identifier, **When** a staff member requests that customer's details, **Then** the system returns a "not found" response.
3. **Given** a customer has been deactivated (soft-deleted), **When** a staff member requests that customer's details, **Then** the system returns a "not found" response.

---

### User Story 3 - Register a New Customer (Priority: P2)

A staff member needs to register a new customer in the system when a walk-in customer wants to start renting, capturing their name, email, and assigning them to a store and address.

**Why this priority**: Adding new customers is essential for business growth but happens less frequently than looking up existing customers.

**Independent Test**: Can be fully tested by submitting a new customer registration request and verifying the customer is created with correct data and a unique identifier is returned.

**Acceptance Scenarios**:

1. **Given** valid customer information is provided (first name, last name, store identifier, address identifier), **When** a staff member submits the registration, **Then** the system creates the customer, marks them as active, and returns the new customer's details with a unique identifier.
2. **Given** required fields are missing (e.g., first name is blank), **When** a staff member submits the registration, **Then** the system rejects the request with specific validation error messages for each invalid field.
3. **Given** an invalid email format is provided, **When** a staff member submits the registration, **Then** the system rejects the request with an email validation error.
4. **Given** a non-existent store identifier is provided, **When** a staff member submits the registration, **Then** the system rejects the request indicating the store does not exist.
5. **Given** a non-existent address identifier is provided, **When** a staff member submits the registration, **Then** the system rejects the request indicating the address does not exist.

---

### User Story 4 - Update Customer Information (Priority: P2)

A staff member needs to update an existing customer's information (name, email, store, or address) when details change, such as a new email address or relocation to a different store.

**Why this priority**: Keeping customer data current is important for operational accuracy, but updates occur less frequently than lookups.

**Independent Test**: Can be fully tested by modifying a customer's details and verifying the changes are persisted and returned correctly.

**Acceptance Scenarios**:

1. **Given** an existing active customer, **When** a staff member updates the customer's email, **Then** the system persists the change and returns the updated customer details.
2. **Given** an existing active customer, **When** a staff member submits an update with invalid data (e.g., blank first name), **Then** the system rejects the request with specific validation error messages.
3. **Given** a non-existent customer identifier, **When** a staff member attempts to update, **Then** the system returns a "not found" response.
4. **Given** a deactivated (soft-deleted) customer, **When** a staff member attempts to update, **Then** the system returns a "not found" response.

---

### User Story 5 - Deactivate a Customer (Priority: P3)

A staff member needs to deactivate a customer's account when they are no longer active (e.g., moved away, requested account closure). The customer record is preserved for historical rental and payment data but is excluded from active customer lists.

**Why this priority**: Deactivation is needed for account lifecycle management but is the least frequent operation. The soft-delete approach preserves data integrity for historical records.

**Independent Test**: Can be fully tested by deactivating a customer and verifying they no longer appear in active customer lists but their record is preserved internally.

**Acceptance Scenarios**:

1. **Given** an existing active customer, **When** a staff member deactivates the customer, **Then** the system marks the customer as inactive and returns a success confirmation.
2. **Given** an already deactivated customer, **When** a staff member attempts to deactivate again, **Then** the system returns a "not found" response.
3. **Given** a non-existent customer identifier, **When** a staff member attempts to deactivate, **Then** the system returns a "not found" response.
4. **Given** a customer has been deactivated, **When** any user browses or searches the customer list, **Then** the deactivated customer does not appear in results.

---

### Edge Cases

- What happens when a search term matches thousands of customers? Pagination caps the response to the requested page size; total count is always included in metadata.
- What happens when page number exceeds available pages? The system returns an empty list with the correct total count.
- What happens when page size is zero or negative? The system rejects the request with a validation error.
- What happens when page size exceeds the maximum allowed? The system caps it at the maximum (100) without error.
- What happens when first name or last name exceeds maximum character length (45)? The system rejects the request with a validation error.
- What happens when email exceeds maximum character length (50)? The system rejects the request with a validation error.
- What happens when email is null/omitted during creation? The system accepts it — email is optional.
- How does the system handle concurrent updates to the same customer? The last write wins; no explicit concurrency control is required for this feature.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a paginated list of active customers, defaulting to page 1 with a page size of 10.
- **FR-002**: System MUST support searching customers by partial match (case-insensitive) on first name, last name, or email using a single search parameter.
- **FR-003**: System MUST return pagination metadata with list results: total record count, total pages, current page number, and page size.
- **FR-004**: System MUST return the full details of a single customer by their unique identifier, including associated store and address identifiers.
- **FR-005**: System MUST return a "not found" response when requesting, updating, or deactivating a customer that does not exist or has been deactivated.
- **FR-006**: System MUST allow creation of a new customer with required fields: first name, last name, store identifier, and address identifier. Email is optional.
- **FR-007**: System MUST validate all input on create and update: first name and last name are required and max 45 characters; email, if provided, must be valid format and max 50 characters; store and address identifiers must reference existing records.
- **FR-008**: System MUST allow updating a customer's first name, last name, email, store identifier, and address identifier.
- **FR-009**: System MUST implement soft delete — deactivation sets the customer's active status to false rather than removing the record.
- **FR-010**: System MUST automatically set the creation date and last-updated timestamp when creating a customer, and update the last-updated timestamp on every modification.
- **FR-011**: System MUST return structured validation error responses that identify each invalid field and the reason for rejection.
- **FR-012**: System MUST never expose internal entity representations in responses — only dedicated response models are returned.
- **FR-013**: System MUST cap the maximum page size at 100 to prevent excessively large responses.

### Key Entities

- **Customer**: The central entity representing a rental customer. Key attributes: unique identifier, first name, last name, optional email, active status, creation date, last update timestamp. Belongs to one Store and one Address.
- **Store**: A rental store location. Customers are assigned to a store. Referenced by identifier in customer data.
- **Address**: A physical address record. Customers are linked to an address. Referenced by identifier in customer data.
- **Paginated Result**: A wrapper for list responses containing the data items plus pagination metadata (total count, total pages, current page, page size).

### Assumptions

- The search parameter applies a single term across first name, last name, and email simultaneously (OR logic) — a customer matches if any of the three fields contain the term.
- Customers are ordered by last name, then first name by default in list results.
- The "active" status for soft delete uses the `activebool` field (boolean) as the primary flag.
- No authentication or authorization is enforced for this feature — that is a separate concern to be added later.
- Address and Store management (CRUD) is out of scope — this feature only references existing addresses and stores by their identifiers.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Staff can retrieve a paginated customer list in under 1 second for up to 10,000 customer records.
- **SC-002**: Staff can search for a customer by partial name or email and receive matching results in under 1 second.
- **SC-003**: Staff can view any individual customer's full details in a single request.
- **SC-004**: Staff can register a new customer and receive confirmation with the customer's unique identifier in a single request.
- **SC-005**: Staff can update any editable customer field and see the changes reflected immediately in subsequent retrievals.
- **SC-006**: Staff can deactivate a customer, and that customer no longer appears in active customer lists.
- **SC-007**: All invalid inputs are rejected with clear, field-specific error messages before any data is persisted.
- **SC-008**: Every acceptance scenario has at least one automated test that verifies the expected behavior end-to-end.
