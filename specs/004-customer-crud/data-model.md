# Data Model: Customer CRUD API

**Feature**: 004-customer-crud | **Date**: 2026-02-22

## Existing Entities (No Changes)

### Customer (table: `customer`)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| CustomerId | int | PK, auto-increment | Column: `customer_id` |
| StoreId | int (smallint) | FK → Store, required | Column: `store_id` |
| FirstName | string | Required, max 45 chars | Column: `first_name` |
| LastName | string | Required, max 45 chars | Column: `last_name` |
| Email | string? | Optional, max 50 chars | Column: `email` |
| AddressId | int (smallint) | FK → Address, required | Column: `address_id` |
| Activebool | bool | Default: true | Column: `activebool` — primary soft-delete flag |
| CreateDate | DateOnly | Default: current date | Column: `create_date` |
| LastUpdate | DateTime? | Default: now() | Column: `last_update` |
| Active | int? | Nullable | Column: `active` — legacy integer flag, kept in sync with Activebool |

**Relationships**:
- Belongs to one **Store** via `StoreId`
- Belongs to one **Address** via `AddressId`
- Has many **Payments** (inverse: `Payment.CustomerId`)
- Has many **Rentals** (inverse: `Rental.CustomerId`)

### Store (table: `store`) — Referenced, not modified

| Field | Type | Constraints |
|-------|------|-------------|
| StoreId | int | PK |
| ManagerStaffId | int (smallint) | FK → Staff |
| AddressId | int (smallint) | FK → Address |

### Address (table: `address`) — Referenced, not modified

| Field | Type | Constraints |
|-------|------|-------------|
| AddressId | int | PK |
| Address1 | string | Required, max 50 chars |
| District | string | Required, max 20 chars |
| CityId | int (smallint) | FK → City |
| Phone | string | Required, max 20 chars |

## New DTOs (Records)

### CustomerResponse

| Field | Type | Maps From |
|-------|------|-----------|
| Id | int | Customer.CustomerId |
| StoreId | int | Customer.StoreId |
| FirstName | string | Customer.FirstName |
| LastName | string | Customer.LastName |
| Email | string? | Customer.Email |
| AddressId | int | Customer.AddressId |
| IsActive | bool | Customer.Activebool |
| CreateDate | DateOnly | Customer.CreateDate |
| LastUpdate | DateTime? | Customer.LastUpdate |

### CreateCustomerRequest

| Field | Type | Validation |
|-------|------|------------|
| FirstName | string | Required, max 45 chars |
| LastName | string | Required, max 45 chars |
| Email | string? | Optional; if provided: valid format, max 50 chars |
| StoreId | int | Required, must reference existing Store |
| AddressId | int | Required, must reference existing Address |

### UpdateCustomerRequest

| Field | Type | Validation |
|-------|------|------------|
| FirstName | string | Required, max 45 chars |
| LastName | string | Required, max 45 chars |
| Email | string? | Optional; if provided: valid format, max 50 chars |
| StoreId | int | Required, must reference existing Store |
| AddressId | int | Required, must reference existing Address |

### PagedResponse\<T\>

| Field | Type | Description |
|-------|------|-------------|
| Items | IReadOnlyList\<T\> | The page of results |
| Page | int | Current page number (1-based) |
| PageSize | int | Items per page |
| TotalCount | int | Total matching records |
| TotalPages | int | Computed: ceil(TotalCount / PageSize) |

## State Transitions

```
[New Customer] --POST--> Active (Activebool=true, Active=1)
                            |
Active --PUT--> Active (fields updated, LastUpdate refreshed)
                            |
Active --DELETE--> Inactive (Activebool=false, Active=0, LastUpdate refreshed)
                            |
Inactive --GET(list)--> [excluded from results]
Inactive --GET(id)--> [404 Not Found]
Inactive --PUT--> [404 Not Found]
Inactive --DELETE--> [404 Not Found]
```

## Validation Rules Summary

| Rule | Applies To | Error Field |
|------|-----------|-------------|
| FirstName required (not empty/whitespace) | Create, Update | `firstName` |
| FirstName max 45 characters | Create, Update | `firstName` |
| LastName required (not empty/whitespace) | Create, Update | `lastName` |
| LastName max 45 characters | Create, Update | `lastName` |
| Email valid format (if provided) | Create, Update | `email` |
| Email max 50 characters (if provided) | Create, Update | `email` |
| StoreId must be > 0 | Create, Update | `storeId` |
| AddressId must be > 0 | Create, Update | `addressId` |
| StoreId must reference existing store | Create, Update | `storeId` (service-level) |
| AddressId must reference existing address | Create, Update | `addressId` (service-level) |
