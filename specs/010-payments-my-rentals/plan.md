# Implementation Plan: Payments & My Rentals Enhancement

**Branch**: `010-payments-my-rentals` | **Date**: 2026-02-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/010-payments-my-rentals/spec.md`

## Summary

Add a Payment CRUD API (POST to record payments, GET to list with role-based filtering) and enhance the existing rental return endpoint to optionally process payment in a single operation. Enrich rental response DTOs with payment summary fields (total paid, rental rate, outstanding balance). On the frontend, enhance the My Rentals page to display payment status per rental, add a "Return & Pay" modal for staff/admin, show per-rental payment history on the detail page, and add a Payments navigation entry for staff/admin users.

The existing `payment` table and `Payment` entity are reused as-is. One EF Core migration is needed to add a nullable `StaffId` FK to `ApplicationUser` (mirroring the existing `CustomerId` pattern) for reliable Identity-to-Staff linking. No new dependencies required.

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (backend), TypeScript 5.9 strict (frontend)
**Primary Dependencies**: ASP.NET Core 10.0, EF Core 10.0 + Npgsql, FluentValidation 11.3, Ardalis.Result 10.1 (backend); React 19.2, TanStack React Query 5.90, Zod 4.3, Shadcn UI (frontend)
**Storage**: PostgreSQL 18 (existing `dvdrental` database, existing `payment` table)
**Testing**: xUnit 2.9 + FluentAssertions 8.8 + AutoFixture 4.18 + Testcontainers.PostgreSql 4.10 (backend); Vitest 4.0 + React Testing Library 16.3 + MSW 2.12 (frontend)
**Target Platform**: Linux (WSL2 development)
**Project Type**: Full-stack web application (monorepo)
**Performance Goals**: Standard web app expectations (< 1s page loads)
**Constraints**: Payment amount max 999.99 (numeric(5,2) DB constraint); StaffId from request body (not JWT); store-scoped filtering via ApplicationUser.StaffId → Staff.StoreId, then Rental → Inventory → Store
**Scale/Scope**: Learning project, no high-traffic requirements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
| --------- | ------ | ----- |
| I. Spec-Driven Development | PASS | Spec approved, clarifications complete, plan follows spec-kit workflow |
| II. Test-First (NON-NEGOTIABLE) | PASS | TDD planned for all production code; every acceptance scenario maps to tests |
| III. Clean Architecture | PASS | Controller → Service → DbContext layering; frontend communicates via REST API only |
| IV. YAGNI and Simplicity | PASS | No new dependencies, reuses existing entity; one migration for ApplicationUser.StaffId (mirrors existing CustomerId pattern); basic payment recording only per spec |
| V. Observability and Maintainability | PASS | Structured logging for payment operations; consistent naming conventions |
| VI. Functional Style and Immutability | PASS | DTOs as records with init-only properties; service returns Result\<T\>; side-effects confined to service methods |
| Controller-based routing | PASS | PaymentsController inherits ControllerBase; no minimal APIs |
| Aggregate validation errors | PASS | FluentValidation + service-level errors aggregated via AsErrors() + List\<ValidationError\> |
| DTO structure (flat, IDs, enum fidelity) | PASS | PaymentListResponse uses IDs; PaymentDetailResponse inlines one-level names; no enums in payment model |
| Result pattern | PASS | IPaymentService returns Result\<T\>/Result for all operations |

**Post-Phase 1 re-check**: All gates still pass. No new abstractions, patterns, or dependencies introduced beyond what's already in the codebase.

## Project Structure

### Documentation (this feature)

```text
specs/010-payments-my-rentals/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── payments-api.md
│   └── rentals-api-enhanced.md
└── tasks.md             # Created by /speckit.tasks
```

### Source Code (repository root)

```text
src/RentalForge.Api/
├── Controllers/
│   ├── PaymentsController.cs          # NEW — Payment CRUD endpoints
│   └── RentalsController.cs           # MODIFIED — Enhanced return endpoint
├── Services/
│   ├── IPaymentService.cs             # NEW — Payment service interface
│   ├── PaymentService.cs              # NEW — Payment business logic
│   ├── IRentalService.cs              # MODIFIED — ReturnRentalAsync signature
│   └── RentalService.cs               # MODIFIED — Return + payment, enhanced queries
├── Validators/
│   ├── CreatePaymentValidator.cs      # NEW
│   └── ReturnRentalValidator.cs       # NEW
├── Models/
│   ├── CreatePaymentRequest.cs        # NEW
│   ├── ReturnRentalRequest.cs         # NEW
│   ├── PaymentListResponse.cs         # NEW
│   ├── PaymentDetailResponse.cs       # NEW
│   ├── RentalPaymentItem.cs           # NEW
│   ├── RentalListResponse.cs          # MODIFIED — Add payment summary fields
│   └── RentalDetailResponse.cs        # MODIFIED — Add payment summary + history
├── Data/
│   ├── Entities/
│   │   └── ApplicationUser.cs         # MODIFIED — Add nullable StaffId FK
│   ├── Migrations/                    # MODIFIED — Add migration for StaffId
│   └── Seeding/
│       └── DevDataSeeder.cs           # MODIFIED — Set StaffId on staff/admin users
└── Program.cs                         # MODIFIED — Register IPaymentService

tests/RentalForge.Api.Tests/
├── PaymentEndpointTests.cs            # NEW
├── PaymentServiceTests.cs             # NEW
├── RentalEndpointTests.cs             # MODIFIED — Tests for enhanced return
├── RentalServiceTests.cs              # MODIFIED — Tests for enhanced return + payment summary
└── Helpers/
    └── PaymentTestHelper.cs           # NEW

src/RentalForge.Web/src/
├── types/
│   ├── payment.ts                     # NEW
│   └── rental.ts                      # MODIFIED — Payment summary fields
├── hooks/
│   ├── use-payments.ts                # NEW
│   └── use-rentals.ts                 # MODIFIED — Enhanced return mutation
├── lib/
│   └── validators.ts                  # MODIFIED — Payment schemas
├── pages/
│   ├── payments-list.tsx              # NEW
│   ├── rentals-list.tsx               # MODIFIED — Payment status display
│   └── rental-detail.tsx              # MODIFIED — Payment history
├── components/
│   ├── payments/
│   │   └── payment-card.tsx           # NEW
│   ├── rentals/
│   │   ├── rental-card.tsx            # MODIFIED — Payment status + Return & Pay
│   │   ├── rental-detail.tsx          # MODIFIED — Payment history section
│   │   └── return-pay-modal.tsx       # NEW
│   └── layout/
│       ├── bottom-nav.tsx             # MODIFIED — Payments nav item
│       └── sidebar-nav.tsx            # MODIFIED — Payments nav item
├── app/
│   └── routes.tsx                     # MODIFIED — Payment routes
└── test/
    ├── mocks/handlers.ts              # MODIFIED — Payment MSW handlers
    └── fixtures/data.ts               # MODIFIED — Payment fixtures
```

**Structure Decision**: Follows the existing monorepo web application structure. All new files slot into established directories matching the patterns from features 004-009. No new directories created except `components/payments/` for payment-specific UI components.

## Complexity Tracking

No constitution violations. No complexity justifications needed.
