# Feature Specification: Result Pattern Refactor

**Feature Branch**: `005-result-pattern-refactor`
**Created**: 2026-02-22
**Status**: Draft
**Input**: User description: "Refactor Customer CRUD API to replace exception-based validation flow (ServiceValidationException) with Ardalis.Result pattern. Service methods should return Result<T>/Result instead of throwing exceptions for expected outcomes (validation failures, not-found, business-rule violations). Controllers should consume Result types and translate to HTTP responses. Add Ardalis.Result, Ardalis.Result.AspNetCore, and Ardalis.Result.FluentValidation NuGet packages. Update CLAUDE.md to reflect the new pattern. All existing tests must continue to pass with equivalent behavior."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Service Layer Returns Typed Results (Priority: P1)

As a developer maintaining the API, I want service methods to
return explicit success/failure results instead of throwing
exceptions, so that error paths are visible in method signatures
and I can compose operations without try/catch blocks.

**Why this priority**: This is the foundational change. Every other
story depends on the service interface returning Result types
instead of using exceptions for expected outcomes. Without this,
controllers and tests cannot be updated.

**Independent Test**: Can be verified by calling each service method
and asserting that validation failures, not-found conditions, and
successful operations all return the correct Result status without
any exceptions being thrown.

**Acceptance Scenarios**:

1. **Given** a create-customer request with an invalid store ID,
   **When** the service processes the request,
   **Then** it returns a failure result containing a validation
   error for the store ID field — no exception is thrown.

2. **Given** a create-customer request with both an invalid store ID
   and an invalid address ID,
   **When** the service processes the request,
   **Then** it returns a failure result containing validation errors
   for both fields in a single response — errors are aggregated,
   not short-circuited.

3. **Given** a request to retrieve a customer by ID that does not
   exist or is inactive,
   **When** the service processes the request,
   **Then** it returns a not-found result — no null return, no
   exception.

4. **Given** a valid create-customer request,
   **When** the service processes the request,
   **Then** it returns a success result wrapping the created
   customer data.

5. **Given** a valid update-customer request for an active customer,
   **When** the service processes the request,
   **Then** it returns a success result wrapping the updated
   customer data.

6. **Given** a deactivation request for an active customer,
   **When** the service processes the request,
   **Then** it returns a success result indicating the customer was
   deactivated.

7. **Given** a deactivation request for a customer that does not
   exist or is already inactive,
   **When** the service processes the request,
   **Then** it returns a not-found result.

---

### User Story 2 - Controller Translates Results to HTTP Responses (Priority: P2)

As an API consumer, I want the HTTP responses to remain identical
to the current behavior (same status codes, same response body
shapes, same validation error format), so that existing clients
are unaffected by the internal refactoring.

**Why this priority**: The controller is the HTTP boundary. It must
translate the new Result types to the same HTTP responses that
clients already depend on. This story cannot begin until the
service layer (US1) returns Result types.

**Independent Test**: Can be verified by running the existing
integration test suite against the API endpoints and confirming
that every test passes without modification (identical HTTP status
codes, response bodies, and validation error shapes).

**Acceptance Scenarios**:

1. **Given** a service result with validation errors,
   **When** the controller translates it to an HTTP response,
   **Then** it returns 400 Bad Request with a ValidationProblemDetails
   body containing all field-level errors.

2. **Given** a service result indicating not-found,
   **When** the controller translates it to an HTTP response,
   **Then** it returns 404 Not Found.

3. **Given** a service result indicating successful creation,
   **When** the controller translates it to an HTTP response,
   **Then** it returns 201 Created with a Location header pointing
   to the new resource and the created entity in the body.

4. **Given** a service result indicating successful update,
   **When** the controller translates it to an HTTP response,
   **Then** it returns 200 OK with the updated entity in the body.

5. **Given** a service result indicating successful deactivation,
   **When** the controller translates it to an HTTP response,
   **Then** it returns 204 No Content.

6. **Given** the full existing integration test suite,
   **When** all tests are executed against the refactored API,
   **Then** every test passes without modification to test
   assertions or expected behaviors.

---

### User Story 3 - Remove Legacy Exception Infrastructure (Priority: P3)

As a developer, I want the legacy exception-based validation
infrastructure (ServiceValidationException and related try/catch
blocks) removed from the codebase, so that there is a single,
consistent error-handling pattern and no dead code.

**Why this priority**: Cleanup can only happen after the service
layer (US1) and controllers (US2) are fully migrated. Leaving
dead code would violate the single-pattern principle and confuse
future developers.

**Independent Test**: Can be verified by confirming the legacy
exception class is deleted, no references to it exist anywhere
in the codebase, and all tests still pass.

**Acceptance Scenarios**:

1. **Given** the service layer and controllers have been migrated
   to Result types,
   **When** the codebase is searched for references to
   ServiceValidationException,
   **Then** zero references are found.

2. **Given** the legacy exception class file exists,
   **When** the migration is complete,
   **Then** the file is deleted from the project.

3. **Given** the cleanup is complete,
   **When** the full test suite is executed,
   **Then** all tests pass.

---

### User Story 4 - Update Project Documentation (Priority: P4)

As a developer onboarding to the project, I want the guidance
documentation to reflect the Result pattern as the standard
error-handling approach, so that I follow the correct conventions
from day one.

**Why this priority**: Documentation updates are non-blocking and
can happen last. The code changes (US1-US3) are the priority.

**Independent Test**: Can be verified by reading the project
guidance file and confirming it references the Result pattern
library, lists it in active technologies, and describes the
error-handling convention in key constraints.

**Acceptance Scenarios**:

1. **Given** the project guidance documentation,
   **When** a developer reads the active technologies section,
   **Then** the Result pattern library and its companion packages
   are listed with their versions.

2. **Given** the project guidance documentation,
   **When** a developer reads the key constraints section,
   **Then** it states that service methods must return Result types
   for expected outcomes instead of throwing exceptions.

---

### Edge Cases

- What happens when a FluentValidation validator returns errors
  and FK existence checks also fail? Both sets of errors must be
  aggregated into a single failure result.
- What happens when the service layer encounters a truly unexpected
  exception (e.g., database connection failure)? It must propagate
  as a thrown exception — Result types are only for expected
  outcomes.
- What happens when controller-level parameter validation fails
  (e.g., invalid page number)? This remains unchanged — controller
  parameter validation does not flow through the service layer.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Service methods MUST return typed result objects
  instead of throwing exceptions for validation failures, not-found
  conditions, and business-rule violations.
- **FR-002**: Service methods MUST aggregate all validation errors
  (input validation and FK existence checks) into a single failure
  result before returning.
- **FR-003**: Each validation error in a failure result MUST carry
  a field identifier and a human-readable error message.
- **FR-004**: The service interface MUST express Result types in
  its method signatures so that callers know at compile time that
  failure is a possible outcome.
- **FR-005**: Controllers MUST translate result objects to the
  appropriate HTTP status codes: validation failure to 400,
  not-found to 404, successful creation to 201, successful
  update to 200, successful deletion to 204.
- **FR-006**: The HTTP response body shape for validation errors
  MUST remain a ValidationProblemDetails-compatible format so
  existing API consumers are unaffected.
- **FR-007**: The FluentValidation bridge MUST convert
  FluentValidation failure results into the Result pattern's
  validation error format.
- **FR-008**: The legacy exception class used for service
  validation errors MUST be removed after migration.
- **FR-009**: All try/catch blocks in controllers that caught the
  legacy validation exception MUST be removed after migration.
- **FR-010**: The project guidance documentation MUST be updated to
  list the Result pattern library in active technologies and
  reference the Result-based error-handling convention in key
  constraints.
- **FR-011**: All existing automated tests MUST pass after the
  refactoring with no changes to test assertions or expected
  behaviors.
- **FR-012**: Unexpected exceptions (e.g., database failures) MUST
  continue to propagate as thrown exceptions — the Result pattern
  applies only to expected outcomes.

### Assumptions

- The refactoring is behavior-preserving: no new API endpoints,
  no changed request/response shapes, no new business rules.
- Controller-level parameter validation (e.g., page/pageSize
  bounds checking) remains as-is — it does not flow through the
  service layer and is not affected by this change.
- The existing integration test suite is the primary verification
  mechanism for behavioral equivalence.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of existing automated tests pass after the
  refactoring with zero modifications to test assertions.
- **SC-002**: Zero references to the legacy validation exception
  class exist in the codebase after migration.
- **SC-003**: Zero try/catch blocks in controllers that catch
  service validation exceptions remain after migration.
- **SC-004**: Every service method that can fail for expected
  reasons returns a typed result object instead of throwing an
  exception.
- **SC-005**: The project guidance documentation references the
  Result pattern library, its companion packages, and the
  error-handling convention.
