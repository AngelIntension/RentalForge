# Data Model: Database Schema Creation & Seeding

**Feature**: 003-db-schema-seeding | **Date**: 2026-02-22

## Entity Overview

All 15 entities already exist in the scaffolded EF Core model. This feature does not add new entities — it adds data seeding configuration to the existing model.

## Reference Data Classification

These 4 tables are populated automatically during schema creation via `HasData()`:

### Country (109 rows)
- **PK**: CountryId (int, identity)
- **Fields**: CountryName (string, 50), LastUpdate (DateTime)
- **Relationships**: Parent of City (1:N)
- **HasData values**: All 109 countries from original dvdrental, IDs 1-109

### City (600 rows)
- **PK**: CityId (int, identity)
- **Fields**: CityName (string, 50), CountryId (int FK), LastUpdate (DateTime)
- **Relationships**: Belongs to Country (N:1), Parent of Address (1:N)
- **HasData values**: All 600 cities with original country associations, IDs 1-600

### Language (6 rows)
- **PK**: LanguageId (int, identity)
- **Fields**: Name (string, 20 fixed-length), LastUpdate (DateTime)
- **Relationships**: Referenced by Film.LanguageId and Film.OriginalLanguageId
- **HasData values**: English (1), Italian (2), Japanese (3), Mandarin (4), French (5), German (6)

### Category (16 rows)
- **PK**: CategoryId (int, identity)
- **Fields**: Name (string, 25), LastUpdate (DateTime)
- **Relationships**: Associated with Film via FilmCategory (M:N)
- **HasData values**: Action (1), Animation (2), Children (3), Classics (4), Comedy (5), Documentary (6), Drama (7), Family (8), Foreign (9), Games (10), Horror (11), Music (12), New (13), Sci-Fi (14), Sports (15), Travel (16)

## Dev Seed Data (11 non-reference tables)

These tables are populated only via the optional `--seed` CLI command:

### Actor (200 rows)
- **PK**: ActorId (int, identity)
- **Fields**: FirstName (string, 45), LastName (string, 45), LastUpdate (DateTime)
- **FK Dependencies**: None
- **Seed tier**: 0 (independent)

### Address (603 rows)
- **PK**: AddressId (int, identity)
- **Fields**: Address1 (string, 50), Address2 (string?, 50), District (string, 20), CityId (int FK), PostalCode (string?, 10), Phone (string, 20), LastUpdate (DateTime)
- **FK Dependencies**: City (reference — already present)
- **Seed tier**: 2

### Film (1,000 rows)
- **PK**: FilmId (int, identity)
- **Fields**: Title (string, 255), Description (string?), ReleaseYear (int?), LanguageId (int FK), OriginalLanguageId (int? FK), RentalDuration (short), RentalRate (decimal 4,2), Length (short?), ReplacementCost (decimal 5,2), Rating (MpaaRating?), LastUpdate (DateTime), SpecialFeatures (string[]?), Fulltext (NpgsqlTsVector)
- **FK Dependencies**: Language (reference — already present)
- **Seed tier**: 2
- **Note**: Fulltext (tsvector) values must be included in seed data or generated post-insert

### Staff (2 rows)
- **PK**: StaffId (int, identity)
- **Fields**: FirstName (string, 45), LastName (string, 45), AddressId (int FK), Email (string?), StoreId (int FK), Active (bool), Username (string, 16), Password (string?), LastUpdate (DateTime), Picture (byte[]?)
- **FK Dependencies**: Address, Store (CIRCULAR)
- **Seed tier**: 3 (requires FK constraint deferral)

### Store (2 rows)
- **PK**: StoreId (int, identity)
- **Fields**: ManagerStaffId (int FK), AddressId (int FK), LastUpdate (DateTime)
- **FK Dependencies**: Staff (CIRCULAR), Address
- **Seed tier**: 4 (requires FK constraint deferral)

### Customer (599 rows)
- **PK**: CustomerId (int, identity)
- **Fields**: StoreId (int FK), FirstName (string, 45), LastName (string, 45), Email (string?), AddressId (int FK), Activebool (bool), CreateDate (DateOnly), LastUpdate (DateTime?), Active (int?)
- **FK Dependencies**: Store, Address
- **Seed tier**: 4

### FilmActor (5,462 rows)
- **PK**: Composite (ActorId, FilmId)
- **Fields**: LastUpdate (DateTime)
- **FK Dependencies**: Actor, Film
- **Seed tier**: 5

### FilmCategory (1,000 rows)
- **PK**: Composite (FilmId, CategoryId)
- **Fields**: LastUpdate (DateTime)
- **FK Dependencies**: Film, Category (reference — already present)
- **Seed tier**: 5

### Inventory (4,581 rows)
- **PK**: InventoryId (int, identity)
- **Fields**: FilmId (int FK), StoreId (int FK), LastUpdate (DateTime)
- **FK Dependencies**: Film, Store
- **Seed tier**: 5

### Rental (16,044 rows)
- **PK**: RentalId (int, identity)
- **Fields**: RentalDate (DateTime), InventoryId (int FK), CustomerId (int FK), ReturnDate (DateTime?), StaffId (int FK), LastUpdate (DateTime)
- **FK Dependencies**: Inventory, Customer, Staff
- **Seed tier**: 6

### Payment (14,596 rows)
- **PK**: PaymentId (int, identity)
- **Fields**: CustomerId (int FK), StaffId (int FK), RentalId (int FK), Amount (decimal 5,2), PaymentDate (DateTime)
- **FK Dependencies**: Customer, Rental, Staff
- **Seed tier**: 7

## Circular Dependency: Staff ↔ Store

```
Staff.StoreId ──FK──> Store.StoreId
Store.ManagerStaffId ──FK──> Staff.StaffId
```

Both FKs are non-nullable. During dev seeding, FK triggers must be disabled using `SET session_replication_role = 'replica'` before bulk insertion and re-enabled after.

## Custom Types

| PostgreSQL Type | EF Core Mapping | Migration Creates |
|-----------------|-----------------|-------------------|
| `mpaa_rating` (enum) | `MpaaRating` C# enum, registered via `NpgsqlDataSourceBuilder.MapEnum<MpaaRating>()` | Yes — Npgsql migration generates `CREATE TYPE mpaa_rating AS ENUM (...)` |
| `year` (domain) | `int?` (Film.ReleaseYear) | No — mapped as plain integer column. Functionally equivalent. |
| `text[]` (array) | `string[]?` (Film.SpecialFeatures) | Yes — Npgsql handles PostgreSQL array types natively |
| `tsvector` | `NpgsqlTsVector` (Film.Fulltext) | Yes — Npgsql handles tsvector type natively |
