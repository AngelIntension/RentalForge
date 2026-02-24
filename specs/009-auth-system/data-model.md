# Data Model: Authentication System

**Feature Branch**: `009-auth-system`
**Date**: 2026-02-24

## New Entities

### ApplicationUser (extends IdentityUser)

**Schema**: `identity` | **Table**: `users`

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | string (GUID) | PK | Inherited from IdentityUser |
| Email | string | Unique, required | Inherited from IdentityUser |
| PasswordHash | string | Required | Inherited from IdentityUser (PBKDF2) |
| UserName | string | Unique, required | Set to Email on creation |
| CustomerId | int? | FK → customer.customer_id, nullable | Links Customer-role users to dvdrental Customer |
| CreatedAt | DateTime | Required, default UTC now | Account creation timestamp |

**Inherited from IdentityUser** (used but not customized): `NormalizedEmail`, `NormalizedUserName`, `EmailConfirmed`, `SecurityStamp`, `ConcurrencyStamp`, `PhoneNumber`, `PhoneNumberConfirmed`, `TwoFactorEnabled`, `LockoutEnd`, `LockoutEnabled`, `AccessFailedCount`.

**Relationships**:
- `ApplicationUser` → `Customer` (optional, 0..1): nullable FK. Only set for Customer-role users.
- `ApplicationUser` → `IdentityRole` (many-to-many via `IdentityUserRole<string>`)
- `ApplicationUser` → `RefreshToken` (one-to-many)

### RefreshToken

**Schema**: `identity` | **Table**: `refresh_tokens`

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | Guid | PK, default gen_random_uuid() | |
| Token | string | Unique, required | Opaque base64 string (64 bytes) |
| Family | string | Required, indexed | UUID identifying the credential family/lineage |
| UserId | string | FK → identity.users.id, required | |
| ExpiresAt | DateTime | Required | Absolute expiry (e.g., 7 days from creation) |
| IsUsed | bool | Required, default false | Set true on rotation (consumed) |
| RevokedAt | DateTime? | Nullable | Set on logout or family invalidation |
| CreatedAt | DateTime | Required, default UTC now | |

**Indexes**:
- `ix_refresh_tokens_token` (unique) on `Token`
- `ix_refresh_tokens_family` on `Family`
- `ix_refresh_tokens_user_id` on `UserId`

**State transitions**:
1. **Active**: `IsUsed = false`, `RevokedAt = null`, `ExpiresAt > now`
2. **Consumed**: `IsUsed = true` (rotated — new token issued in same family)
3. **Revoked**: `RevokedAt != null` (logout or family invalidation)
4. **Expired**: `ExpiresAt <= now` (natural expiry)

### Identity Framework Tables (standard, no customization)

All in `identity` schema:

| Table | Purpose |
|-------|---------|
| `roles` | IdentityRole — three seeded: Admin, Staff, Customer |
| `user_roles` | IdentityUserRole<string> — user-to-role mapping |
| `user_claims` | IdentityUserClaim<string> |
| `user_logins` | IdentityUserLogin<string> |
| `user_tokens` | IdentityUserToken<string> |
| `role_claims` | IdentityRoleClaim<string> |

## Modified Entities

### Customer (existing — no schema changes)

No changes to the `Customer` entity itself. The relationship is owned by `ApplicationUser.CustomerId` pointing to `Customer.CustomerId`. The `Customer` entity gains a navigation property:

```
Customer.AuthUser → ApplicationUser? (optional inverse navigation)
```

## Seed Data

### Roles (HasData in OnModelCreating)

| Id (GUID) | Name | NormalizedName |
|-----------|------|----------------|
| (generated) | Admin | ADMIN |
| (generated) | Staff | STAFF |
| (generated) | Customer | CUSTOMER |

### Default Users (dev seeding via CLI `--seed`)

| Email | Role | CustomerId | Notes |
|-------|------|------------|-------|
| admin@rentalforge.dev | Admin | null | Full access |
| staff@rentalforge.dev | Staff | null | Operations access |
| customer@rentalforge.dev | Customer | (existing customer ID) | Linked to a dvdrental Customer record |

Password for all seeded users: `RentalForge1!` (meets strength requirements: 8+ chars, upper, lower, digit, special).

## Validation Rules

### Registration

| Field | Rule |
|-------|------|
| Email | Required, valid email format, unique (not already registered) |
| Password | Required, min 8 chars, at least 1 uppercase, 1 lowercase, 1 digit, 1 non-alphanumeric |
| Role | Optional (defaults to Customer). If provided as Staff or Admin, requester must be authenticated Admin. |

### Login

| Field | Rule |
|-------|------|
| Email | Required, valid email format |
| Password | Required, non-empty |

### Refresh

| Field | Rule |
|-------|------|
| RefreshToken | Required, non-empty |

## Entity Relationship Diagram (text)

```
identity.users (ApplicationUser)
  ├── PK: Id (string)
  ├── FK: CustomerId → public.customer.customer_id (nullable)
  ├── 1:N → identity.refresh_tokens (via UserId)
  └── M:N → identity.roles (via identity.user_roles)

identity.refresh_tokens
  ├── PK: Id (Guid)
  └── FK: UserId → identity.users.Id

identity.roles
  └── PK: Id (string)

public.customer (existing)
  ├── PK: customer_id (int)
  └── 0..1 ← identity.users.CustomerId
```
