# Feature Specification: ASP.NET Core Controller Refactor

**Feature Branch**: `002-aspnet-core-refactor`
**Created**: 2026-02-21
**Status**: Draft
**Input**: User description: "Refactor the solution to use ASP.NET core web framework."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Controller-Based Health Endpoint (Priority: P1)

As a developer consuming the RentalForge API, I want the existing
health endpoint to be served from a controller class so that API
routing follows a consistent, discoverable controller-based pattern
across the entire application.

**Why this priority**: The health endpoint is the only existing
endpoint. Migrating it to a controller is the minimum viable
refactor that proves the new architecture works end-to-end — from
routing through to OpenAPI documentation and integration tests.

**Independent Test**: Can be fully tested by issuing a GET request
to the health endpoint and verifying the response body, status
codes, and OpenAPI documentation remain identical to the current
behavior.

**Acceptance Scenarios**:

1. **Given** the API is running, **When** a client sends
   `GET /health` with a reachable database, **Then** the response
   is 200 OK with status "healthy", a database version string,
   and a server timestamp — identical to the current behavior.
2. **Given** the API is running, **When** a client sends
   `GET /health` with an unreachable database, **Then** the
   response is 503 Service Unavailable with status "unhealthy"
   and an error message — identical to the current behavior.
3. **Given** the API is running, **When** a developer opens the
   Swagger UI, **Then** the health endpoint appears with the same
   operation name, summary, and response schema as today.

---

### User Story 2 - ASP.NET Core Controller Infrastructure (Priority: P2)

As a developer extending the RentalForge API, I want the project
to be configured for controller-based routing so that all future
endpoints follow the same architectural pattern without additional
setup.

**Why this priority**: Infrastructure setup is a prerequisite for
all future controller work but has no user-visible behavior on its
own. It is lower priority than the health endpoint migration
because its value is only realized when combined with at least one
working controller.

**Independent Test**: Can be verified by confirming that the
application starts, controller discovery is active, and the
minimal API endpoint extension pattern is fully removed from the
codebase.

**Acceptance Scenarios**:

1. **Given** the application startup configuration, **When** the
   app builds and runs, **Then** controller-based routing is
   active and controllers are discovered automatically.
2. **Given** the refactored codebase, **When** a developer
   searches for minimal API endpoint registrations
   (`app.Map*` calls for business endpoints), **Then** none
   exist.
3. **Given** the refactored codebase, **When** a developer
   searches for the `Endpoints/` directory, **Then** it no
   longer exists.

---

### Edge Cases

- What happens if controller routing is registered but no
  controllers are discovered? The application MUST still start
  without errors.
- What happens if the health endpoint database query times out?
  Behavior MUST remain unchanged from the current implementation
  (503 with error message).
- What happens if Swagger UI is accessed after the refactor?
  All endpoint metadata MUST render identically to the current
  minimal API documentation.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The health endpoint (`GET /health`) MUST return
  identical response bodies and status codes (200/503) as the
  current minimal API implementation.
- **FR-002**: The health endpoint MUST be implemented as a
  controller action within a class inheriting from
  `ControllerBase`.
- **FR-003**: The application MUST use `AddControllers()` and
  `MapControllers()` for routing instead of minimal API endpoint
  extension methods.
- **FR-004**: The `Endpoints/` directory and its contents MUST be
  removed after migration.
- **FR-005**: OpenAPI/Swagger documentation MUST remain functional
  with equivalent operation metadata (operation name, summary,
  response types).
- **FR-006**: All existing integration tests MUST continue to
  pass without modification to their assertion logic (test
  infrastructure may be updated if needed).
- **FR-007**: The `HealthResponse` record MUST be preserved as
  the response contract (no breaking changes to its shape).

### Assumptions

- The refactor is limited to converting the existing minimal API
  pattern to controller-based routing. No new endpoints or
  business logic are added.
- The EF Core context registration, Npgsql data source
  configuration, and user-secrets setup remain unchanged.
- The test project's `TestWebAppFactory` infrastructure may
  require minor adjustments to support controller discovery but
  the test scenarios themselves remain the same.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All existing integration tests pass with zero
  failures after the refactor.
- **SC-002**: The health endpoint returns identical response
  payloads for both healthy and unhealthy scenarios compared to
  the pre-refactor baseline.
- **SC-003**: Swagger UI displays the health endpoint with
  equivalent documentation (operation name, description, response
  schemas).
- **SC-004**: Zero minimal API endpoint registrations
  (`app.MapGet`, `app.MapPost`, etc.) remain in the codebase for
  business endpoints.
- **SC-005**: The application starts and responds to requests
  within the same timeframe as the pre-refactor baseline (no
  measurable startup regression).
