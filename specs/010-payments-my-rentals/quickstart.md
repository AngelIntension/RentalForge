# Quickstart: Payments & My Rentals Enhancement

**Feature Branch**: `010-payments-my-rentals`
**Date**: 2026-02-25

## Prerequisites

- .NET 10.0 SDK installed
- Node.js (LTS) + npm installed
- PostgreSQL 18 running at localhost:5432 with `dvdrental` database
- User secrets configured for connection string and JWT keys

## Backend

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run API with dev data seeding
dotnet run --project src/RentalForge.Api -- --seed
```

API available at http://localhost:5089

## Frontend

```bash
cd src/RentalForge.Web
npm install
npm run dev
```

Dev server at http://localhost:5173

## New Files (Backend)

| File | Purpose |
| ---- | ------- |
| `src/RentalForge.Api/Controllers/PaymentsController.cs` | Payment CRUD endpoints |
| `src/RentalForge.Api/Services/IPaymentService.cs` | Payment service interface |
| `src/RentalForge.Api/Services/PaymentService.cs` | Payment business logic |
| `src/RentalForge.Api/Validators/CreatePaymentValidator.cs` | Payment creation validation |
| `src/RentalForge.Api/Validators/ReturnRentalValidator.cs` | Return-with-payment validation |
| `src/RentalForge.Api/Models/CreatePaymentRequest.cs` | Payment creation DTO |
| `src/RentalForge.Api/Models/ReturnRentalRequest.cs` | Enhanced return DTO |
| `src/RentalForge.Api/Models/PaymentListResponse.cs` | Payment list DTO |
| `src/RentalForge.Api/Models/PaymentDetailResponse.cs` | Payment detail DTO |
| `src/RentalForge.Api/Models/RentalPaymentItem.cs` | Inline payment item DTO |
| `tests/RentalForge.Api.Tests/PaymentEndpointTests.cs` | Payment integration tests |
| `tests/RentalForge.Api.Tests/PaymentServiceTests.cs` | Payment service unit tests |
| `tests/RentalForge.Api.Tests/Helpers/PaymentTestHelper.cs` | Payment test data seeding |

## Modified Files (Backend)

| File | Change |
| ---- | ------ |
| `src/RentalForge.Api/Data/Entities/ApplicationUser.cs` | Add nullable StaffId FK + Staff navigation property |
| `src/RentalForge.Api/Data/Migrations/` | Add migration for ApplicationUser.StaffId |
| `src/RentalForge.Api/Data/Seeding/DevDataSeeder.cs` | Set StaffId on staff/admin seed users |
| `src/RentalForge.Api/Services/IRentalService.cs` | `ReturnRentalAsync` signature adds optional request parameter |
| `src/RentalForge.Api/Services/RentalService.cs` | Enhanced return logic + payment summary in queries |
| `src/RentalForge.Api/Controllers/RentalsController.cs` | Return endpoint accepts optional body |
| `src/RentalForge.Api/Models/RentalListResponse.cs` | Add TotalPaid, RentalRate, OutstandingBalance |
| `src/RentalForge.Api/Models/RentalDetailResponse.cs` | Add payment summary + Payments list |
| `src/RentalForge.Api/Program.cs` | Register IPaymentService |

## New Files (Frontend)

| File | Purpose |
| ---- | ------- |
| `src/RentalForge.Web/src/types/payment.ts` | Payment TypeScript types |
| `src/RentalForge.Web/src/hooks/use-payments.ts` | Payment data hooks |
| `src/RentalForge.Web/src/pages/payments-list.tsx` | Payments list page (Staff/Admin) |
| `src/RentalForge.Web/src/components/payments/payment-card.tsx` | Payment list card |
| `src/RentalForge.Web/src/components/rentals/return-pay-modal.tsx` | Return & Pay modal form |

## Modified Files (Frontend)

| File | Change |
| ---- | ------- |
| `src/RentalForge.Web/src/types/rental.ts` | Add payment summary fields to rental types |
| `src/RentalForge.Web/src/hooks/use-rentals.ts` | Enhanced return mutation with optional payment |
| `src/RentalForge.Web/src/lib/validators.ts` | Add createPaymentSchema, returnPaySchema |
| `src/RentalForge.Web/src/app/routes.tsx` | Add /payments route |
| `src/RentalForge.Web/src/components/layout/bottom-nav.tsx` | Add Payments nav item (Staff/Admin) |
| `src/RentalForge.Web/src/components/layout/sidebar-nav.tsx` | Add Payments nav item (Staff/Admin) |
| `src/RentalForge.Web/src/components/rentals/rental-card.tsx` | Show payment status, Return & Pay button |
| `src/RentalForge.Web/src/components/rentals/rental-detail.tsx` | Show payment history section |
| `src/RentalForge.Web/src/pages/rentals-list.tsx` | Enhanced rental cards with payment info |
| `src/RentalForge.Web/src/pages/rental-detail.tsx` | Payment history display |
| `src/RentalForge.Web/src/test/mocks/handlers.ts` | Add payment MSW handlers |
| `src/RentalForge.Web/src/test/fixtures/data.ts` | Add payment fixtures |

## Key Implementation Notes

- Payment entity and table already exist — no changes needed
- One migration needed: add nullable `StaffId` FK to `ApplicationUser` (mirrors existing `CustomerId` pattern)
- StaffId on payment requests comes from request body, not JWT (consistent with existing rental creation pattern)
- Store-scoped filtering for Staff: ApplicationUser.StaffId → Staff.StoreId, then Payment → Rental → Inventory.StoreId
- `numeric(5,2)` limits payment amount to max 999.99
- MSW handlers must use regex patterns (not string paths) per project convention
- Zod imports from `'zod/v4'` (not `'zod'`)
