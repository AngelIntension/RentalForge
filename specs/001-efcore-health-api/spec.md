# Feature Specification: EF Core Scaffold and Health API

**Feature Branch**: `001-efcore-health-api`
**Created**: 2026-02-21
**Status**: Draft
**Input**: User description: "Scaffold EF Core DbContext from the existing dvdrental PostgreSQL database running on localhost:5432. Create a minimal ASP.NET Core Minimal API project with a /health endpoint that returns database version and current time. Use appsettings.json + user-secrets for connection string. Include xUnit test project with one integration test using Testcontainers."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Database Health Check (Priority: P1)

A developer starting the application wants to verify that the API
can connect to the dvdrental PostgreSQL database. They navigate to
the `/health` endpoint and receive a response containing the
database server version and the current server time, confirming
that the application is properly configured and the database
connection is live.

**Why this priority**: This is the foundational verification
story. Without a working database connection, no other feature in
the system can function. It proves the entire stack end-to-end:
project structure, configuration, database access, and HTTP
serving.

**Independent Test**: Can be fully tested by sending a GET request
to `/health` and verifying the response contains a database
version string and a valid timestamp. Delivers immediate
confidence that the infrastructure is operational.

**Acceptance Scenarios**:

1. **Given** the API is running and the database is reachable,
   **When** a GET request is sent to `/health`,
   **Then** the response status is 200 OK and the body contains
   the database version and current server time.

2. **Given** the API is running but the database is unreachable,
   **When** a GET request is sent to `/health`,
   **Then** the response status indicates a service problem (503)
   and the body contains an actionable error message explaining
   the database is unavailable.

3. **Given** the API is running and the database is reachable,
   **When** a GET request is sent to `/health`,
   **Then** the response is returned within 2 seconds under
   normal conditions.

---

### User Story 2 - Scaffolded Data Access Layer (Priority: P2)

A developer wants to query the existing dvdrental database tables
(e.g., films, customers, rentals) through a typed data access
layer. The existing database schema is represented as entity
classes and a database context so that future features can query
and manipulate dvdrental data without writing raw SQL.

**Why this priority**: The scaffolded data access layer is the
prerequisite for every future feature that reads or writes
dvdrental data. Without it, no business logic features can be
built. It is lower priority than the health check because the
health check validates the infrastructure the scaffold depends on.

**Independent Test**: Can be tested by instantiating the database
context against a test database, querying a known table (e.g.,
films), and verifying that results are returned with the expected
shape and types.

**Acceptance Scenarios**:

1. **Given** a database containing the dvdrental schema,
   **When** a developer queries the films table through the data
   access layer,
   **Then** film records are returned with all expected attributes
   (title, description, release year, rental rate, etc.).

2. **Given** the scaffolded data access layer,
   **When** a developer inspects the entity classes,
   **Then** every table in the dvdrental schema has a corresponding
   entity with properties matching the database columns.

3. **Given** the scaffolded data access layer,
   **When** a developer inspects the database context,
   **Then** all entity sets are registered and navigations between
   related entities (e.g., film to category, customer to rental)
   are configured.

---

### User Story 3 - Automated Integration Test (Priority: P3)

A developer wants to run an automated integration test that
verifies the `/health` endpoint works against a real database
without depending on a shared development database. The test
spins up an isolated database instance, runs the health check,
and verifies the response — all without manual setup.

**Why this priority**: Automated testing is essential for
regression prevention, but it depends on the health endpoint
(US1) being in place first. The /health endpoint uses server-
level SQL queries and does not depend on the data access layer
(US2).

**Independent Test**: Can be tested by running the test suite.
The test provisions its own database, starts the API against it,
calls `/health`, and asserts on the response. No external
dependencies required beyond a container runtime.

**Acceptance Scenarios**:

1. **Given** a container runtime (Docker) is available,
   **When** the integration test suite is executed,
   **Then** a temporary database is automatically provisioned,
   the API starts against it, the `/health` endpoint is called,
   and the response is verified to contain a database version
   and current time.

2. **Given** the integration test has completed,
   **When** the test process exits,
   **Then** the temporary database container is automatically
   cleaned up with no residual resources.

---

### Edge Cases

- What happens when the database connection string is missing or
  malformed? The application MUST fail fast at startup with a
  clear error message indicating the configuration problem.
- What happens when the database exists but the dvdrental schema
  is empty or missing expected tables? The health endpoint MUST
  still return the database version and time (these are server-
  level queries, not schema-dependent).
- What happens when the container runtime is unavailable during
  integration tests? The test MUST fail with a clear message
  indicating Docker is required, not with a cryptic timeout.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose a `/health` endpoint that accepts
  GET requests.
- **FR-002**: The `/health` response MUST include the PostgreSQL
  server version string.
- **FR-003**: The `/health` response MUST include the current
  database server timestamp.
- **FR-004**: The `/health` endpoint MUST return HTTP 200 when the
  database is reachable and HTTP 503 when it is not.
- **FR-005**: The `/health` response MUST use a structured format
  (JSON) with clearly named fields for version and time.
- **FR-006**: System MUST include a data access layer with entity
  classes representing all tables in the dvdrental schema.
- **FR-007**: System MUST load the database connection string from
  application configuration, supporting the standard configuration
  hierarchy (user-secrets override appsettings.json).
- **FR-008**: System MUST fail fast at startup with an actionable
  error if the connection string is missing or empty.
- **FR-009**: System MUST include at least one automated
  integration test that verifies the `/health` endpoint against an
  isolated, disposable database instance.
- **FR-010**: The integration test MUST provision and tear down its
  own database with no manual intervention.
- **FR-011**: The `/health` endpoint MUST include API documentation
  metadata so it appears in the generated API specification.

### Key Entities

- **Health Check Response**: Represents the result of a database
  connectivity check. Contains: database version (text), current
  server time (timestamp), status indicator (healthy/unhealthy).
- **Dvdrental Entities**: Entity representations of all existing
  dvdrental database tables — including but not limited to: Actor,
  Address, Category, City, Country, Customer, Film, FilmActor,
  FilmCategory, Inventory, Language, Payment, Rental, Staff, Store.
  Each entity mirrors its database table's columns and
  relationships.

### Assumptions

- The dvdrental database is the standard PostgreSQL sample database
  available from postgresqltutorial.com.
- The database is accessible at `localhost:5432` during
  development. Integration tests use their own containerized
  database and do not depend on this.
- Docker (or a compatible container runtime) is available on the
  development machine for running Testcontainers.
- The health endpoint does not require authentication — it is a
  public diagnostic endpoint.
- The `/health` endpoint queries the database directly (e.g.,
  `SELECT version()` and `SELECT NOW()`) and does not depend on
  the dvdrental schema being present.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The `/health` endpoint returns a valid response
  containing database version and server time within 2 seconds
  under normal operating conditions.
- **SC-002**: The `/health` endpoint returns a 503 status with an
  actionable error message within 5 seconds when the database is
  unreachable.
- **SC-003**: The application starts successfully and serves the
  `/health` endpoint when configured with a valid connection
  string.
- **SC-004**: The application fails to start with an actionable
  error message when the connection string is missing.
- **SC-005**: The integration test suite completes in under 60
  seconds, including container provisioning and teardown.
- **SC-006**: All dvdrental tables are represented in the data
  access layer with correct column mappings and relationships.
- **SC-007**: The `/health` endpoint appears in the auto-generated
  API documentation with request/response descriptions.
