# API Contract: Films

**Branch**: `006-film-crud` | **Date**: 2026-02-23
**Base path**: `/api/films`

---

## GET /api/films

**Summary**: List films with optional search, filtering, and pagination.

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| search | string | No | null | Case-insensitive partial match on title, description, or actor first/last name (OR logic) |
| category | string | No | null | Case-insensitive exact match on category name |
| rating | string | No | null | Exact match on MPAA rating (G, PG, PG-13, R, NC-17) |
| yearFrom | int | No | null | Minimum release year (inclusive) |
| yearTo | int | No | null | Maximum release year (inclusive) |
| page | int | No | 1 | Page number (≥ 1) |
| pageSize | int | No | 10 | Items per page (1–100, capped at 100) |

All filters combine with AND logic.

### Responses

**200 OK**
```json
{
  "items": [
    {
      "id": 1,
      "title": "Academy Dinosaur",
      "description": "A Epic Drama...",
      "releaseYear": 2006,
      "languageId": 1,
      "originalLanguageId": null,
      "rentalDuration": 6,
      "rentalRate": 0.99,
      "length": 86,
      "replacementCost": 20.99,
      "rating": "PG",
      "specialFeatures": ["Deleted Scenes", "Behind the Scenes"],
      "lastUpdate": "2026-02-15T09:46:27Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1000,
  "totalPages": 100
}
```

**400 Bad Request** (invalid page/pageSize/yearFrom > yearTo)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "page": ["'Page' must be greater than or equal to '1'."],
    "yearFrom": ["'Year From' must be less than or equal to 'Year To'."]
  }
}
```

---

## GET /api/films/{id}

**Summary**: Get full film details by ID, including actors, categories, and language names.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | Film identifier |

### Responses

**200 OK**
```json
{
  "id": 1,
  "title": "Academy Dinosaur",
  "description": "A Epic Drama of a Feminist And a Mad Scientist...",
  "releaseYear": 2006,
  "languageId": 1,
  "languageName": "English",
  "originalLanguageId": null,
  "originalLanguageName": null,
  "rentalDuration": 6,
  "rentalRate": 0.99,
  "length": 86,
  "replacementCost": 20.99,
  "rating": "PG",
  "specialFeatures": ["Deleted Scenes", "Behind the Scenes"],
  "lastUpdate": "2026-02-15T09:46:27Z",
  "actors": ["Penelope Guiness", "Christian Gable", "Lucille Tracy"],
  "categories": ["Documentary"]
}
```

**404 Not Found** (film does not exist)

---

## POST /api/films

**Summary**: Create a new film.

### Request Body

```json
{
  "title": "New Film Title",
  "description": "Optional description",
  "releaseYear": 2026,
  "languageId": 1,
  "originalLanguageId": null,
  "rentalDuration": 5,
  "rentalRate": 3.99,
  "length": 120,
  "replacementCost": 24.99,
  "rating": "PG-13",
  "specialFeatures": ["Trailers", "Commentaries"]
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| title | string | Yes | Max 255 characters |
| description | string | No | Max 1000 characters |
| releaseYear | int | No | 1888–(current year + 5) |
| languageId | int | Yes | Must reference existing language |
| originalLanguageId | int | No | Must reference existing language |
| rentalDuration | short | Yes | > 0 |
| rentalRate | decimal | Yes | > 0 |
| length | short | No | > 0 |
| replacementCost | decimal | Yes | > 0 |
| rating | string | No | G, PG, PG-13, R, NC-17 (string or numeric accepted) |
| specialFeatures | string[] | No | Array of strings |

### Responses

**201 Created**
- Body: `FilmDetailResponse` (same shape as GET /api/films/{id})
- Header: `Location: /api/films/{newId}`

**400 Bad Request** (validation errors)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "title": ["'Title' must not be empty."],
    "languageId": ["Language with ID 999 does not exist."],
    "rentalRate": ["'Rental Rate' must be greater than '0'."]
  }
}
```

---

## PUT /api/films/{id}

**Summary**: Update an existing film (full replacement).

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | Film identifier |

### Request Body

Same shape and constraints as POST /api/films.

### Responses

**200 OK**
- Body: `FilmDetailResponse` (updated film with related data)

**400 Bad Request** (validation errors — same format as POST)

**404 Not Found** (film does not exist)

---

## DELETE /api/films/{id}

**Summary**: Permanently delete a film (hard delete).

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | Film identifier |

### Responses

**204 No Content** (success — film and join table rows deleted)

**404 Not Found** (film does not exist)

**409 Conflict** (film has associated inventory records)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Cannot delete film with ID 1 because it has associated inventory records."
}
```
