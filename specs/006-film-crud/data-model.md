# Data Model: Film CRUD API

**Branch**: `006-film-crud` | **Date**: 2026-02-23

## Existing Entities (read-only — no schema changes)

All entities below already exist in the `dvdrental` database and are
scaffolded as EF Core entity classes. No migrations needed.

### Film (primary entity)

| Column | Type | Nullable | Constraints | Notes |
|--------|------|----------|-------------|-------|
| film_id | int (serial) | NO | PK | Auto-generated |
| title | varchar(255) | NO | | Required, max 255 |
| description | text | YES | | Optional |
| release_year | int | YES | | Optional, valid year |
| language_id | smallint | NO | FK → language | Required |
| original_language_id | smallint | YES | FK → language | Optional |
| rental_duration | smallint | NO | | Default 3, must be > 0 |
| rental_rate | numeric(4,2) | NO | | Default 4.99, must be > 0 |
| length | smallint | YES | | Optional, must be > 0 |
| replacement_cost | numeric(5,2) | NO | | Default 19.99, must be > 0 |
| rating | mpaa_rating | YES | | PostgreSQL enum: G, PG, PG-13, R, NC-17 |
| last_update | timestamp | NO | | Auto-set on create/update |
| special_features | text[] | YES | | PostgreSQL text array |
| fulltext | tsvector | NO | | DB-managed, not exposed via API |

**Relationships:**
- `Film → Language` (many-to-one, required) via `language_id`
- `Film → Language` (many-to-one, optional) via `original_language_id`
- `Film → FilmActor → Actor` (many-to-many)
- `Film → FilmCategory → Category` (many-to-many)
- `Film → Inventory` (one-to-many) — blocks hard delete

### FilmActor (join table)

| Column | Type | Nullable | Constraints |
|--------|------|----------|-------------|
| actor_id | int | NO | PK (composite), FK → actor |
| film_id | int | NO | PK (composite), FK → film |
| last_update | timestamp | NO | |

**Cascade**: Deleted when parent Film is deleted.

### FilmCategory (join table)

| Column | Type | Nullable | Constraints |
|--------|------|----------|-------------|
| film_id | int | NO | PK (composite), FK → film |
| category_id | int | NO | PK (composite), FK → category |
| last_update | timestamp | NO | |

**Cascade**: Deleted when parent Film is deleted.

### Actor (read-only reference)

| Column | Type | Nullable | Constraints |
|--------|------|----------|-------------|
| actor_id | int (serial) | NO | PK |
| first_name | varchar(45) | NO | |
| last_name | varchar(45) | NO | |
| last_update | timestamp | NO | |

### Category (read-only reference)

| Column | Type | Nullable | Constraints |
|--------|------|----------|-------------|
| category_id | int (serial) | NO | PK |
| name | varchar(25) | NO | |
| last_update | timestamp | NO | |

### Language (read-only reference)

| Column | Type | Nullable | Constraints |
|--------|------|----------|-------------|
| language_id | int (serial) | NO | PK |
| name | char(20) | NO | |
| last_update | timestamp | NO | |

### Inventory (blocks deletion)

| Column | Type | Nullable | Constraints |
|--------|------|----------|-------------|
| inventory_id | int (serial) | NO | PK |
| film_id | smallint | NO | FK → film (RESTRICT) |
| store_id | smallint | NO | FK → store |
| last_update | timestamp | NO | |

---

## DTO Models (new — to be created)

### FilmListResponse (lean list item)

```text
record FilmListResponse(
    int Id,
    string Title,
    string? Description,
    int? ReleaseYear,
    int LanguageId,
    int? OriginalLanguageId,
    short RentalDuration,
    decimal RentalRate,
    short? Length,
    decimal ReplacementCost,
    string? Rating,             ← MpaaRating enum serialized as string
    string[]? SpecialFeatures,
    DateTime LastUpdate
)
```

**Design rationale**: Constitution v1.8.0 — IDs only for related
entities, no embedded names. Rating serialized as string per enum
converter rule.

### FilmDetailResponse (rich detail)

```text
record FilmDetailResponse(
    int Id,
    string Title,
    string? Description,
    int? ReleaseYear,
    int LanguageId,
    string LanguageName,             ← flat, one-level relationship
    int? OriginalLanguageId,
    string? OriginalLanguageName,    ← flat, one-level relationship
    short RentalDuration,
    decimal RentalRate,
    short? Length,
    decimal ReplacementCost,
    string? Rating,                  ← MpaaRating enum serialized as string
    string[]? SpecialFeatures,
    DateTime LastUpdate,
    IReadOnlyList<string> Actors,    ← flat list: "FirstName LastName"
    IReadOnlyList<string> Categories ← flat list: category names
)
```

**Design rationale**: Constitution v1.8.0 — one-level relationships
inlined as flat properties. Actors and categories as string lists
(not nested DTOs). Language ID + name as top-level properties.

### CreateFilmRequest

```text
record CreateFilmRequest {
    string Title              ← required, max 255
    string? Description       ← optional, max 1000
    int? ReleaseYear          ← optional, valid year range
    int LanguageId            ← required, FK existence checked in service
    int? OriginalLanguageId   ← optional, FK existence checked in service
    short RentalDuration      ← required, > 0
    decimal RentalRate        ← required, > 0
    short? Length              ← optional, > 0
    decimal ReplacementCost   ← required, > 0
    string? Rating            ← optional, valid MpaaRating string
    string[]? SpecialFeatures ← optional, string array
}
```

### UpdateFilmRequest

```text
record UpdateFilmRequest {
    string Title              ← required, max 255
    string? Description       ← optional, max 1000
    int? ReleaseYear          ← optional, valid year range
    int LanguageId            ← required, FK existence checked in service
    int? OriginalLanguageId   ← optional, FK existence checked in service
    short RentalDuration      ← required, > 0
    decimal RentalRate        ← required, > 0
    short? Length              ← optional, > 0
    decimal ReplacementCost   ← required, > 0
    string? Rating            ← optional, valid MpaaRating string
    string[]? SpecialFeatures ← optional, string array
}
```

---

## Validation Rules

### FluentValidation (CreateFilmValidator / UpdateFilmValidator)

| Field | Rule | Message |
|-------|------|---------|
| Title | NotEmpty, MaxLength(255) | Default FluentValidation messages |
| Description | MaxLength(1000) when not null | |
| ReleaseYear | InclusiveBetween(1888, currentYear+5) when not null | |
| LanguageId | GreaterThan(0) | |
| OriginalLanguageId | GreaterThan(0) when not null | |
| RentalDuration | GreaterThan(0) | |
| RentalRate | GreaterThan(0) | |
| Length | GreaterThan(0) when not null | |
| ReplacementCost | GreaterThan(0) | |
| Rating | Must(be valid MpaaRating) when not null | |

### Service-Layer Validation (FK existence)

| Check | Error |
|-------|-------|
| LanguageId exists in languages table | "Language with ID {id} does not exist." |
| OriginalLanguageId exists in languages table (when provided) | "Original language with ID {id} does not exist." |

Both FluentValidation errors and FK errors are aggregated into a
single `Result<T>.Invalid(allErrors)` response.

---

## State Transitions

Film has no state machine — it is either present or deleted (hard
delete). No soft-delete flag, no activation/deactivation lifecycle.

| Trigger | Before | After |
|---------|--------|-------|
| POST /api/films | Not exists | Exists with all fields set |
| PUT /api/films/{id} | Exists | Exists with updated fields + new LastUpdate |
| DELETE /api/films/{id} | Exists (no inventory) | Permanently removed |
| DELETE /api/films/{id} | Exists (has inventory) | Unchanged — 409 Conflict |
