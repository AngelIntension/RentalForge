# Feature Specification: Film CRUD API

**Feature Branch**: `006-film-crud`
**Created**: 2026-02-23
**Status**: Draft
**Input**: User description: "Implement full RESTful CRUD + rich search/pagination for the Film entity with flexible search across title, description, release year, rating, category name, and actor name. Detail endpoint includes related data (actors, categories, language). No soft delete."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse and Search Films (Priority: P1)

A staff member needs to browse the film catalog and search for films using flexible criteria — by title, description keywords, MPAA rating, category name, actor name, or release year range — to quickly locate films for customer inquiries, inventory management, or catalog browsing.

**Why this priority**: Browsing and searching is the most frequently performed operation. Staff and customers need to find films before they can do anything else (view details, manage inventory, process rentals).

**Independent Test**: Can be fully tested by sending search and filter queries to the film list endpoint and verifying that results are filtered, paginated, and returned in the expected format.

**Acceptance Scenarios**:

1. **Given** the system has film records, **When** a user requests the film list without filters, **Then** the system returns a paginated list of films with default page size. Each list item includes core film fields (title, description, release year, language identifier, rental duration, rental rate, length, replacement cost, rating, special features) but not related entity names — those are available only via the detail endpoint.
2. **Given** the system has film records, **When** a user searches by a partial title, **Then** the system returns only films whose title contains the search term (case-insensitive).
3. **Given** the system has film records, **When** a user searches by a description keyword, **Then** the system returns only films whose description contains the search term (case-insensitive).
4. **Given** the system has film records, **When** a user filters by category name, **Then** the system returns only films that belong to the specified category (case-insensitive exact match).
5. **Given** the system has film records, **When** a user filters by MPAA rating, **Then** the system returns only films with the specified rating.
6. **Given** the system has film records, **When** a user searches by an actor name, **Then** the system returns only films featuring an actor whose first or last name contains the search term (case-insensitive).
7. **Given** the system has film records, **When** a user filters by a release year range (e.g., yearFrom=2000, yearTo=2010), **Then** the system returns only films with a release year within that range (inclusive).
8. **Given** the system has film records, **When** a user provides only yearFrom without yearTo, **Then** the system returns films with a release year greater than or equal to yearFrom.
9. **Given** the system has film records, **When** a user provides only yearTo without yearFrom, **Then** the system returns films with a release year less than or equal to yearTo.
10. **Given** the system has film records, **When** a user combines a search term with a category filter, **Then** the system returns only films matching both criteria (AND logic).
11. **Given** the system has film records, **When** a user requests page 2 with a page size of 10, **Then** the system returns the second page of results with up to 10 records and includes pagination metadata (total count, total pages, current page).
12. **Given** no films match the search criteria, **When** a user performs a search, **Then** the system returns an empty list with zero total count.

---

### User Story 2 - View Film Details (Priority: P1)

A staff member needs to view the full details of a specific film, including its actors, categories, and language information, to provide complete information to customers or verify catalog data.

**Why this priority**: Viewing individual film details with related data is a core operation needed for any film inquiry — equally critical as search.

**Independent Test**: Can be fully tested by requesting a specific film by identifier and verifying all expected fields are returned, including related actor names, category names, and language.

**Acceptance Scenarios**:

1. **Given** a film exists in the system, **When** a user requests that film's details, **Then** the system returns the film's full information including title, description, release year, language identifier and language name (flat), original language identifier and original language name (flat, if set), rental duration, rental rate, length, replacement cost, rating, special features, and last update timestamp.
2. **Given** a film exists with associated actors, **When** a user requests that film's details, **Then** the response includes a list of actor names (first and last name) for all actors in the film.
3. **Given** a film exists with associated categories, **When** a user requests that film's details, **Then** the response includes a list of category names for all categories assigned to the film.
4. **Given** no film exists with the requested identifier, **When** a user requests that film's details, **Then** the system returns a "not found" response.

---

### User Story 3 - Add a New Film (Priority: P2)

A staff member needs to add a new film to the catalog when new titles become available, capturing all required film metadata and optionally associating it with a language.

**Why this priority**: Adding new films is essential for catalog growth but happens less frequently than browsing or viewing existing films.

**Independent Test**: Can be fully tested by submitting a new film creation request and verifying the film is created with correct data and a unique identifier is returned.

**Acceptance Scenarios**:

1. **Given** valid film information is provided (title, language identifier, rental duration, rental rate, replacement cost), **When** a staff member submits the creation request, **Then** the system creates the film and returns the new film's details with a unique identifier.
2. **Given** optional fields are provided (description, release year, original language identifier, length, rating, special features), **When** a staff member submits the creation request, **Then** the system persists all provided fields.
3. **Given** required fields are missing (e.g., title is blank), **When** a staff member submits the creation request, **Then** the system rejects the request with specific validation error messages for each invalid field.
4. **Given** a non-existent language identifier is provided, **When** a staff member submits the creation request, **Then** the system rejects the request indicating the language does not exist.
5. **Given** a non-existent original language identifier is provided, **When** a staff member submits the creation request, **Then** the system rejects the request indicating the original language does not exist.
6. **Given** an invalid MPAA rating value is provided, **When** a staff member submits the creation request, **Then** the system rejects the request with a validation error for the rating field.
7. **Given** multiple validation errors exist in the request, **When** a staff member submits the creation request, **Then** the system returns all validation errors in a single response (aggregated, not early-return).

---

### User Story 4 - Update Film Information (Priority: P2)

A staff member needs to update an existing film's metadata when details change, such as correcting a description, updating the rental rate, or changing the assigned language.

**Why this priority**: Keeping catalog data current is important for operational accuracy, but updates occur less frequently than lookups.

**Independent Test**: Can be fully tested by modifying a film's details and verifying the changes are persisted and returned correctly.

**Acceptance Scenarios**:

1. **Given** an existing film, **When** a staff member updates the film's title and rental rate, **Then** the system persists the changes and returns the updated film details.
2. **Given** an existing film, **When** a staff member submits an update with invalid data (e.g., blank title, negative rental rate), **Then** the system rejects the request with specific validation error messages.
3. **Given** a non-existent film identifier, **When** a staff member attempts to update, **Then** the system returns a "not found" response.
4. **Given** an existing film, **When** a staff member updates the language to a non-existent language identifier, **Then** the system rejects the request indicating the language does not exist.

---

### User Story 5 - Remove a Film (Priority: P3)

A staff member needs to remove a film from the catalog when it is no longer available or was added in error. This is a hard delete — the record is permanently removed.

**Why this priority**: Film removal is the least frequent operation and is typically an administrative cleanup task.

**Independent Test**: Can be fully tested by deleting a film and verifying it no longer appears in search results or can be retrieved by identifier.

**Acceptance Scenarios**:

1. **Given** an existing film with no inventory records, **When** a staff member deletes the film, **Then** the system permanently removes the film record (and its actor/category join records) and returns a success confirmation.
2. **Given** an existing film with associated inventory records, **When** a staff member attempts to delete, **Then** the system returns a conflict error indicating the film has dependent inventory records.
3. **Given** a non-existent film identifier, **When** a staff member attempts to delete, **Then** the system returns a "not found" response.
4. **Given** a film has been deleted, **When** any user searches for or requests that film, **Then** the film does not appear in results and the detail request returns "not found".

---

### Edge Cases

- What happens when a search term matches thousands of films? Pagination caps the response to the requested page size; total count is always included in metadata.
- What happens when page number exceeds available pages? The system returns an empty list with the correct total count.
- What happens when page size is zero or negative? The system rejects the request with a validation error.
- What happens when page size exceeds the maximum allowed? The system caps it at the maximum (100) without error.
- What happens when title exceeds maximum character length (255)? The system rejects the request with a validation error.
- What happens when description exceeds maximum character length (1000)? The system rejects the request with a validation error.
- What happens when release year is outside a reasonable range (e.g., before 1888 or after current year + 5)? The system rejects the request with a validation error.
- What happens when rental duration, rental rate, or replacement cost is zero or negative? The system rejects the request with a validation error.
- What happens when an invalid rating string is provided? The system rejects the request with a validation error listing the valid rating values.
- What happens when a film being deleted has associated actor or category join records? Those join records are cascade-deleted automatically along with the film.
- What happens when a film being deleted has associated inventory records? Deletion is blocked; the system surfaces a meaningful conflict error rather than an unhandled exception.
- What happens when the search term is provided alongside both a category and rating filter? All filters apply together using AND logic — the film must match the search term AND belong to the category AND have the specified rating.
- What happens when yearFrom is greater than yearTo? The system rejects the request with a validation error.
- What happens when yearFrom or yearTo is not a valid integer? The system rejects the request with a validation error.

## Clarifications

### Session 2026-02-23

- Q: Should deleting a film cascade-remove join table rows (film-actor, film-category), or should those block deletion too? → A: Cascade join tables on delete; only inventory records block deletion.
- Q: Should release year be a filterable parameter, and if so, what format? → A: Add `yearFrom` and `yearTo` range filter pair for flexible year range queries.
- Q: What fields should the list DTO include vs. the detail DTO? → A: Lean list (core film fields + language ID only); actor names, category names, and language name reserved for detail endpoint. Per constitution v1.8.0 DTO structure rules.
- Constitution v1.8.0 alignment: MPAA rating enum MUST accept both string and numeric representations. Detail DTO MUST include language/original language IDs alongside names (flat structure). List DTO returns IDs for related entities, not embedded data.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a paginated list of films, defaulting to page 1 with a page size of 10. Each list item MUST include core film fields (identifier, title, description, release year, language identifier, rental duration, rental rate, length, replacement cost, rating, special features, last update timestamp) but MUST NOT include related entity names (actor names, category names, language name) — those belong exclusively in the detail response.
- **FR-002**: System MUST support searching films by partial match (case-insensitive) on title and description using a single `search` query parameter. A film matches if either field contains the search term.
- **FR-003**: System MUST support filtering films by category name using a `category` query parameter (case-insensitive exact match on category name).
- **FR-004**: System MUST support filtering films by MPAA rating using a `rating` query parameter (exact match on rating value: G, PG, PG-13, R, NC-17). The rating filter MUST accept the string representation of the rating.
- **FR-005**: System MUST support filtering films by release year range using `yearFrom` and `yearTo` query parameters (inclusive). Either parameter may be used independently: `yearFrom` alone returns films from that year onward; `yearTo` alone returns films up to and including that year; both together define an inclusive range.
- **FR-006**: System MUST support searching films by actor name using the `search` query parameter — a film matches if any of its actors' first or last names contain the search term (case-insensitive).
- **FR-007**: System MUST apply all provided filters using AND logic — a film must satisfy every active filter to appear in results.
- **FR-008**: System MUST return pagination metadata with list results: total record count, total pages, current page number, and page size.
- **FR-009**: System MUST return the full details of a single film by its unique identifier, including: title, description, release year, language identifier and language name (flat), original language identifier and original language name (flat, if set), rental duration, rental rate, film length, replacement cost, MPAA rating, special features, last update timestamp, a list of actor names (flat list of strings), and a list of category names (flat list of strings).
- **FR-010**: System MUST return a "not found" response when requesting, updating, or deleting a film that does not exist.
- **FR-011**: System MUST allow creation of a new film with required fields: title, language identifier, rental duration, rental rate, and replacement cost. Optional fields: description, release year, original language identifier, length, rating, and special features.
- **FR-012**: System MUST validate all input on create and update: title is required and max 255 characters; description, if provided, max 1000 characters; release year, if provided, must be a valid year; rental duration must be positive; rental rate must be positive; replacement cost must be positive; length, if provided, must be positive; rating, if provided, must be a valid MPAA rating value; language identifier must reference an existing record; original language identifier, if provided, must reference an existing record.
- **FR-013**: System MUST allow updating a film's title, description, release year, language, original language, rental duration, rental rate, length, replacement cost, rating, and special features.
- **FR-014**: System MUST implement hard delete — deletion permanently removes the film record and its associated actor and category join records (cascade). Deletion MUST be blocked if the film has associated inventory records; the system returns a meaningful conflict error in that case.
- **FR-015**: System MUST automatically update the last-updated timestamp on every modification.
- **FR-016**: System MUST return structured validation error responses that identify each invalid field and the reason for rejection.
- **FR-017**: System MUST never expose internal entity representations in responses — only dedicated response models are returned.
- **FR-018**: System MUST cap the maximum page size at 100 to prevent excessively large responses.
- **FR-019**: System MUST order film list results by title alphabetically by default.
- **FR-020**: MPAA rating MUST accept both string (e.g., "PG-13") and numeric representations in request payloads and MUST serialize as the string representation in responses (per constitution v1.8.0 enum converter rule).
- **FR-021**: Response models MUST return identifiers (IDs) for related entities wherever possible. Related entity names (language name, actor names, category names) MUST only be included where the spec explicitly requires them (detail endpoint), and MUST be represented as flat, inlined properties — not nested objects (per constitution v1.8.0 DTO structure rules).

### Key Entities

- **Film**: The central entity representing a title in the rental catalog. Key attributes: unique identifier, title, optional description, optional release year, language, optional original language, rental duration, rental rate, optional length (minutes), replacement cost, optional MPAA rating (G, PG, PG-13, R, NC-17), optional special features, last update timestamp. Related to actors and categories through many-to-many relationships.
- **Actor**: A person who appears in films. Key attributes: unique identifier, first name, last name. Related to films through a many-to-many join.
- **Category**: A genre classification for films (e.g., Action, Comedy, Drama). Key attributes: unique identifier, name. Related to films through a many-to-many join.
- **Language**: A language associated with a film. Key attributes: unique identifier, name. Films have a required primary language and an optional original language.
- **Paginated Result**: A wrapper for list responses containing the data items plus pagination metadata (total count, total pages, current page, page size).

### Assumptions

- The `search` parameter applies a single term across title, description, and actor names simultaneously (OR logic within search) — a film matches if any of these fields contain the term. The `category` and `rating` parameters are separate, dedicated filters that combine with `search` using AND logic.
- Films are ordered by title alphabetically by default in list results.
- No authentication or authorization is enforced for this feature — that is a separate concern to be added later.
- Actor and Category management (CRUD) is out of scope — this feature only reads existing actors and categories for display in film details. Film-to-actor and film-to-category assignments are out of scope for create/update (only film metadata fields are modifiable).
- Language management (CRUD) is out of scope — this feature only references existing languages by their identifiers.
- Inventory management is out of scope — no stock logic, no inventory creation on film creation.
- The `fulltext` tsvector column is managed by the database and is not exposed or modified through the API.
- Special features are represented as a list of strings (matching the PostgreSQL text array column).
- When deleting a film, join table records (film-actor, film-category) are cascade-deleted. Only inventory records block deletion — the system surfaces this as a meaningful conflict error.
- DTO structure follows constitution v1.8.0: list responses are lean (IDs for related entities, no embedded names); detail responses include related names as flat inlined properties (not nested objects); enum properties accept both string and numeric values.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can retrieve a paginated film list in under 1 second for up to 10,000 film records.
- **SC-002**: Users can search for films by title, description, or actor name and receive matching results in under 1 second.
- **SC-003**: Users can filter films by category and/or rating and receive matching results in under 1 second.
- **SC-004**: Users can view any individual film's full details — including actor names, category names, and language — in a single request.
- **SC-005**: Users can add a new film to the catalog and receive confirmation with the film's unique identifier in a single request.
- **SC-006**: Users can update any editable film field and see the changes reflected immediately in subsequent retrievals.
- **SC-007**: Users can permanently remove a film from the catalog, and the film no longer appears in any search results or detail requests.
- **SC-008**: All invalid inputs are rejected with clear, field-specific error messages before any data is persisted.
- **SC-009**: Every acceptance scenario has at least one automated test that verifies the expected behavior end-to-end.
