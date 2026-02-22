# Feature Specification: Database Schema Creation & Seeding

**Feature Branch**: `003-db-schema-seeding`
**Created**: 2026-02-22
**Status**: Draft
**Input**: User description: "Build a mechanism for creating a mostly empty version of the existing dvdrental database. Identify data which is appropriate to be automatically inserted initially, such as countries, cities, etc. and include a mechanism for inserting that data on creation. Build an optional mechanism for seeding this new database with its current contents for development environments. The optional seeding operation should be invokable via command line."

## Clarifications

### Session 2026-02-22

- Q: Where should the seed data physically come from at runtime? → A: Embedded in project. Both reference data and full development seed data are stored as static data within the codebase. No runtime dependency on the original dvdrental database.
- Q: What should happen when the dev seed command is run on an already-seeded database? → A: Default to skip with notification; provide a force/reset flag to clear and re-seed. Developers get both safe default behavior and explicit reset capability.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create Empty Database with Reference Data (Priority: P1)

As a developer setting up a new environment, I want to create a fresh database with the dvdrental schema and pre-populated reference data so that the application is immediately usable without manually importing lookup tables.

When a new database is created, the schema (all tables, relationships, constraints, and custom types) is applied automatically. Reference data — information that rarely changes and is required for the application to function — is inserted during creation. This includes:

- **Countries**: The full set of countries (e.g., United States, Canada, United Kingdom, etc.)
- **Cities**: All cities associated with their respective countries
- **Languages**: The set of film languages (e.g., English, Italian, Japanese, Mandarin, French, German)
- **Categories**: Film genre categories (e.g., Action, Animation, Comedy, Drama, Horror, etc.)

After creation, the database has all tables present and empty except for these four reference tables, which are fully populated.

**Why this priority**: Without the schema and reference data, no other part of the application can function. Reference data is a prerequisite for creating films, addresses, stores, and customers. This is the foundational capability.

**Independent Test**: Can be fully tested by triggering database creation against an empty database instance and verifying that all 15 tables exist, the four reference tables contain the expected row counts, and all other tables are empty.

**Acceptance Scenarios**:

1. **Given** an empty database server with no dvdrental database, **When** the database creation mechanism is invoked, **Then** all 15 tables are created with correct columns, constraints, foreign keys, and custom types (e.g., the MPAA rating type).
2. **Given** the database schema has just been created, **When** the reference data insertion completes, **Then** the Country table contains all countries from the original dvdrental database, the City table contains all cities with correct country associations, the Language table contains all languages, and the Category table contains all categories.
3. **Given** a newly created database with reference data, **When** a user queries any non-reference table (Actor, Film, Store, Staff, Customer, Address, Inventory, Rental, Payment, FilmActor, FilmCategory), **Then** the table exists but contains zero rows.

---

### User Story 2 - Seed Full Development Data via Command Line (Priority: P2)

As a developer working in a development environment, I want to optionally seed the database with the complete dvdrental dataset by running a single command so that I have realistic data for testing, debugging, and feature development.

After the empty database with reference data exists (User Story 1), a developer can invoke a command-line operation to populate all remaining tables with the full dvdrental sample data. This includes actors, films, film-actor associations, film-category associations, stores, staff, customers, addresses, inventory, rentals, and payments. The seeding operation preserves all relationships and data integrity.

**Why this priority**: Development seeding depends on the schema and reference data from P1. It provides a realistic dataset for development and manual testing but is not required for the application to start or for automated tests to run.

**Independent Test**: Can be fully tested by first creating a database with reference data (P1), then invoking the seed command and verifying that all tables contain the expected row counts matching the original dvdrental database.

**Acceptance Scenarios**:

1. **Given** a database that has been created with schema and reference data (P1 complete), **When** the developer runs the seed command from the command line, **Then** all tables are populated with the complete dvdrental dataset and all foreign key relationships are intact.
2. **Given** a database with schema and reference data, **When** the developer runs the seed command, **Then** the operation completes and reports success, including a summary of records inserted per table.
3. **Given** a database that has already been fully seeded, **When** the developer runs the seed command without a force flag, **Then** the system detects existing data, skips seeding, and reports that data already exists with no changes made.
4. **Given** a database that has already been fully seeded, **When** the developer runs the seed command with the force/reset flag, **Then** the system clears all non-reference data and re-seeds from the embedded dataset, reporting the number of records inserted per table.

---

### User Story 3 - Idempotent and Safe Operations (Priority: P3)

As a developer, I want the database creation and seeding operations to be safe to run repeatedly so that I don't accidentally corrupt data or encounter errors when re-running setup steps.

Both the schema creation and the seeding operations should handle pre-existing state gracefully. If the schema already exists, creation should not fail or destroy existing data. If reference data already exists, it should not be duplicated. If the full seed has already been applied, the seed command should handle this cleanly.

**Why this priority**: Idempotency is important for developer experience but is secondary to the core creation and seeding functionality. It ensures setup scripts can be safely re-run during iterative development.

**Independent Test**: Can be fully tested by running creation and seeding operations multiple times in sequence and verifying that the database state remains correct and consistent after each run.

**Acceptance Scenarios**:

1. **Given** a database where the schema already exists, **When** the creation mechanism is invoked again, **Then** the schema is not duplicated or corrupted, and the operation completes without errors.
2. **Given** a database where reference data already exists, **When** the reference data insertion runs again, **Then** no duplicate rows are created and the existing data remains intact.
3. **Given** a fully seeded database, **When** the seed command is run again without the force flag, **Then** the system skips seeding without errors or data corruption. **When** run with the force flag, **Then** the system clears and re-seeds without errors, leaving the database in a consistent state.

---

### Edge Cases

- What happens when the database server is unreachable during creation or seeding?
- What happens when seeding is attempted on a database that has no schema (P1 was never run)?
- What happens when the connection string is missing or invalid?
- What happens when the seed operation is interrupted mid-way (e.g., network drop, process killed)?
- What happens when the target database already contains partial data (e.g., some tables seeded, others empty)?
- What happens when the embedded seed data is incomplete or corrupted?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST create the complete dvdrental database schema including all 15 tables, foreign key relationships, constraints, indexes, and custom types (e.g., MPAA rating enumeration).
- **FR-002**: System MUST automatically populate reference data tables (Country, City, Language, Category) during schema creation, with all rows matching the original dvdrental database.
- **FR-003**: System MUST leave all non-reference tables empty after initial creation (Actor, Film, FilmActor, FilmCategory, Store, Staff, Customer, Address, Inventory, Rental, Payment).
- **FR-004**: System MUST provide a command-line invokable operation that seeds the database with the complete dvdrental dataset for development use.
- **FR-005**: The development seeding operation MUST populate all non-reference tables with data matching the original dvdrental database, preserving all relationships and referential integrity.
- **FR-006**: The development seeding operation MUST be clearly marked and documented as a development-only capability, not intended for production use.
- **FR-007**: The development seeding command MUST provide feedback on progress and completion, including the number of records inserted per table.
- **FR-008**: The schema creation mechanism MUST be idempotent — running it against a database that already has the schema should not produce errors or data loss.
- **FR-009**: The reference data insertion MUST be idempotent — running it when reference data already exists should not create duplicate rows.
- **FR-010**: The development seeding operation MUST default to skipping if data already exists, reporting that no changes were made. A force/reset option MUST be available that clears all non-reference data and re-seeds from the embedded dataset.
- **FR-011**: All operations MUST provide clear error messages when encountering failures (unreachable server, invalid connection, missing prerequisites).

### Key Entities

- **Country**: Geographic reference data — country name; parent of City
- **City**: Geographic reference data — city name; belongs to a Country; parent of Address
- **Language**: Film language reference data — language name; referenced by Film
- **Category**: Film genre reference data — category name; associated with Film via FilmCategory
- **Actor**: Film performer — first name, last name; associated with Film via FilmActor
- **Film**: Film catalog entry — title, description, release year, rental rate, rating, special features; belongs to Language; associated with Actor and Category through join tables
- **FilmActor**: Association between Film and Actor (many-to-many)
- **FilmCategory**: Association between Film and Category (many-to-many)
- **Store**: Rental location — managed by Staff; has an Address; holds Inventory
- **Staff**: Store employee — name, credentials, active status; belongs to Store and Address
- **Customer**: Rental customer — name, email, active status; belongs to Store and Address
- **Address**: Physical location — street, district, postal code, phone; belongs to City
- **Inventory**: Physical film copy at a store — belongs to Film and Store
- **Rental**: Transaction record — rental and return dates; ties Customer, Inventory, and Staff
- **Payment**: Financial record — amount, date; ties Customer, Staff, and Rental

## Assumptions

- The reference data (Country, City, Language, Category) from the original dvdrental database is considered stable and appropriate for all environments, not just development.
- All seed data (both reference data and full development data) is embedded within the project as static data. There is no runtime dependency on the original dvdrental database. The original database serves only as the initial source from which the embedded data was extracted.
- The command-line seeding operation targets the same database instance the application connects to, using the configured connection string.
- Tables must be seeded in dependency order to satisfy foreign key constraints (e.g., Country before City, City before Address, Address before Store/Staff/Customer, etc.).
- The development seed operation is expected to run against a local or development database only; no safeguards against running in production are required beyond clear documentation and naming.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can create a new database with the complete schema and reference data in a single operation that completes in under 30 seconds.
- **SC-002**: After initial creation, all 4 reference tables contain data matching the original dvdrental database, and all 11 non-reference tables are empty.
- **SC-003**: A developer can seed the complete dvdrental dataset by running a single command-line operation that completes in under 60 seconds.
- **SC-004**: After full development seeding, all 15 tables contain row counts matching the original dvdrental database with all relationships intact.
- **SC-005**: Running the creation or seeding operations multiple times produces no errors and leaves the database in a consistent, correct state.
- **SC-006**: The seeding command provides clear progress output, and a developer can understand what happened (records inserted per table, success/failure status) without inspecting the database directly.
