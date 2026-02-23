# Research: Film CRUD API

**Branch**: `006-film-crud` | **Date**: 2026-02-23

## Research Topics & Decisions

### 1. Multi-Table Search Strategy

**Decision**: Use EF Core LINQ joins with `EF.Functions.ILike()` for
case-insensitive search across Film title, description, and Actor names.

**Rationale**: The dvdrental dataset has ~1,000 films and ~200 actors.
At this scale, EF Core's translated SQL with `ILIKE` is more than
sufficient. The existing Customer search uses the same `ILike` pattern.

**Alternatives considered**:
- PostgreSQL full-text search (`ts_vector`/`ts_query`): Over-engineered
  for ~1K records. The `fulltext` tsvector column exists on Film but is
  database-managed and doesn't cover actor names. Would require custom
  SQL or raw queries.
- Application-side filtering: Loads too much data. Rejected for
  correctness (pagination counts would be wrong if filtered in-memory).

### 2. Category and Rating Filter Implementation

**Decision**: Category filter uses a join to `FilmCategory` →
`Category` with case-insensitive exact match on `Category.Name`.
Rating filter uses direct `WHERE` clause on `Film.Rating` enum column.
Year range filters use `>=` / `<=` on `Film.ReleaseYear`.

**Rationale**: Direct SQL translation by EF Core. Category is a
join-based filter (many-to-many); rating and year are scalar column
filters. All combine with AND logic in a single `IQueryable` pipeline.

**Alternatives considered**:
- Separate endpoints per filter type: Violates YAGNI. One list
  endpoint with composable query parameters is simpler and standard.

### 3. Detail Response with Related Data (Flat DTO)

**Decision**: Detail endpoint eagerly loads `FilmActors → Actor` and
`FilmCategories → Category` and `Language` + `OriginalLanguage` via
EF Core `Include()` or projected `Select()`. Maps to flat DTO:
`LanguageId` + `LanguageName` as top-level properties,
`Actors` as `IReadOnlyList<string>`, `Categories` as
`IReadOnlyList<string>`.

**Rationale**: Constitution v1.8.0 mandates flat DTOs for one-level
relationships. Actor and category names are string lists (not nested
objects). Language is inlined as ID + name pair. Using `Select()`
projection in EF Core avoids loading full entity graphs.

**Alternatives considered**:
- Nested DTOs (`ActorDto { Id, FirstName, LastName }`): Violates
  constitution v1.8.0 flat DTO rule for one-level relationships.
- Separate endpoints for actors/categories: Over-engineered per YAGNI.

### 4. Hard Delete with Cascade Behavior

**Decision**: Service layer checks for inventory records via
`db.Inventories.AnyAsync(i => i.FilmId == id)`. If inventory exists,
return `Result.Conflict()`. Otherwise, delete film — EF Core cascade
deletes `FilmActor` and `FilmCategory` join rows automatically
(configured via `OnDelete(DeleteBehavior.Cascade)` or database
default).

**Rationale**: Per spec clarification, join tables cascade but
inventory blocks. Checking inventory first gives a meaningful error.
The dvdrental schema has `ON DELETE RESTRICT` on `inventory.film_id`,
so the DB would reject the delete anyway — but checking first lets
us return a clean 409 Conflict instead of an unhandled DB exception.

**Alternatives considered**:
- Let DB throw and catch the exception: Violates constitution v1.8.0
  Result pattern (exceptions for expected outcomes prohibited).
- Cascade everything: Rejected per spec — inventory represents real
  business objects.

### 5. Enum Serialization (MpaaRating)

**Decision**: Configure `JsonStringEnumConverter` on the `MpaaRating`
enum (or on the DTO properties) to accept both string ("PG-13") and
numeric (2) representations. Serialize as string in responses.

**Rationale**: Constitution v1.8.0 requires enum properties in DTOs
to use a JSON enum converter that accepts both numeric and string
values. The existing `MpaaRating` enum has `[EnumMember]` attributes
mapping to display values ("G", "PG", "PG-13", "R", "NC-17").

**Alternatives considered**:
- String-only field with manual parsing: Loses type safety. Rejected.
- Global converter in Program.cs: Could affect other enums
  unintentionally. Prefer per-property or per-type attribute.

### 6. List vs Detail DTO Separation

**Decision**: Two response types:
- `FilmListResponse`: Core film fields + `LanguageId` (no names,
  no actor/category data). Used by GET list endpoint.
- `FilmDetailResponse`: All film fields + `LanguageId` +
  `LanguageName` + `OriginalLanguageId` + `OriginalLanguageName` +
  `Actors` (string list) + `Categories` (string list). Used by
  GET by ID and returned from POST/PUT.

**Rationale**: Constitution v1.8.0 mandates lean list DTOs (IDs for
related entities) and flat detail DTOs (inlined names). Separating
the two keeps list payloads small and avoids N+1 queries on the
list endpoint.

**Alternatives considered**:
- Single DTO for both: Would either bloat list responses or starve
  detail responses. Rejected.
- List includes category names: Clarification chose lean list (Option A).

### 7. Test Data Seeding for Films

**Decision**: Create a `FilmTestHelper` class (mirroring
`CustomerTestHelper`) that seeds: languages, categories, actors,
films, film_actor joins, film_category joins, and optionally
inventory records for delete-blocking tests.

**Rationale**: Film tests need related data across multiple tables.
A dedicated helper keeps test setup DRY and consistent. Uses raw SQL
with `session_replication_role = 'replica'` for FK cycle workarounds
where needed (matching existing pattern).

**Alternatives considered**:
- Inline seeding per test class: Too much duplication. Rejected.
- Shared database state: Violates test isolation principle.
