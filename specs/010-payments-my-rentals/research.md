# Research: Payments & My Rentals Enhancement

**Feature Branch**: `010-payments-my-rentals`
**Date**: 2026-02-25

## Decision 1: Payment Entity Schema

**Decision**: Use the existing `payment` table as-is. A migration is needed only for `ApplicationUser.StaffId` (see Decision 2a).

**Rationale**: The `payment` table already exists in the dvdrental database with the exact fields needed: `payment_id` (PK), `customer_id` (FK → customer), `staff_id` (FK → staff), `rental_id` (FK → rental), `amount` (numeric(5,2)), `payment_date` (timestamp). The Payment entity class and DbContext configuration are already in place. EF Core navigation properties (Customer, Staff, Rental) are configured.

**Alternatives considered**:
- Add a `last_update` column to payment → Rejected: not needed for MVP, would require a migration, violates YAGNI.
- Add a `payment_type` enum column → Rejected: the spec explicitly excludes multiple payment types.

## Decision 2: StaffId Resolution for Payments

**Decision**: StaffId is passed in the request body for payment creation, not resolved from JWT claims. This is consistent with the existing `CreateRentalRequest` pattern.

**Rationale**: Payment creation requires a staff ID that may differ from the authenticated user (e.g., an admin recording a payment on behalf of a staff member). The service layer validates that the staff member exists and is active. Consistent with `CreateRentalRequest` which also requires `StaffId` as a client-provided field.

**Alternatives considered**:
- Add staffId claim to JWT → Rejected: would require changes to token generation, refresh flow, and all existing tests. Disproportionate change for this feature.
- Look up staff by email match → Rejected: fragile coupling between Identity and Staff tables; Staff.Username != ApplicationUser.Email in all cases.

## Decision 2a: ApplicationUser.StaffId for Identity-to-Staff Linking

**Decision**: Add a nullable `StaffId` property to `ApplicationUser`, mirroring the existing `CustomerId` pattern. This requires a single EF Core migration.

**Rationale**: Server-side store-scoped filtering (Decision 3) requires resolving the authenticated user's staff record. `ApplicationUser` already has `CustomerId` for customer-role users — adding `StaffId` is the symmetric pattern for staff-role users. This provides a reliable FK-based link instead of fragile email-matching between Identity and dvdrental tables. The seeder sets `StaffId` on staff/admin users during dev data setup.

**Alternatives considered**:
- Look up staff by email match → Rejected: fragile coupling; Staff.Email may not match ApplicationUser.Email in all cases (same concern from Decision 2). A direct FK is more reliable and forward-compatible.
- No migration (keep email lookup as workaround) → Rejected: contradicts the project's own rejection of email-based staff resolution in Decision 2.

## Decision 3: Store-Scoped Payment Filtering for Staff

**Decision**: Resolve staff's store via `ApplicationUser.StaffId` → `Staff.StoreId`, then filter payments where `Payment.Rental.Inventory.StoreId == staffStoreId`.

**Rationale**: Staff members see all payments at their store (per clarification). The store association path is: Payment → Rental → Inventory.StoreId. The staff member's own StoreId comes from the Staff entity. The controller resolves the staff record via `ApplicationUser.StaffId` (Decision 2a), then queries `Staff.StoreId`. This follows the same pattern as `GetCurrentUserCustomerId()` which uses `ApplicationUser.CustomerId` — the new `GetCurrentUserStoreId()` helper uses `ApplicationUser.StaffId` → `Staff.StoreId`.

Server-side resolution ensures clients cannot control their own visibility scope.

**Alternatives considered**:
- Client-provided storeId filter → Rejected: security risk, clients could bypass store scoping.
- Email-based staff lookup → Rejected: fragile coupling (see Decision 2, Decision 2a).
- No store scoping (staff sees all) → Rejected: contradicts clarification decision.

## Decision 4: ReturnRental Enhancement Pattern

**Decision**: Add an optional `ReturnRentalRequest` record with a nullable `Amount` and `StaffId` property. The existing `ReturnRentalAsync(int id)` signature changes to `ReturnRentalAsync(int id, ReturnRentalRequest? request)`.

**Rationale**: When `request` is null or `request.Amount` is null, behavior is identical to the current implementation (backward compatible per FR-010). When `request.Amount` is provided, a Payment record is created in the same transaction. The `StaffId` on the request identifies who processed the return+payment.

**Alternatives considered**:
- Separate `ReturnAndPayAsync` method → Rejected: creates parallel code paths for the same domain operation; violates DRY.
- Two sequential API calls (return then create payment) → Rejected: not atomic, poor UX, contradicts spec requirement for single-action combined flow.

## Decision 5: Enhanced Rental DTOs for Payment Summary

**Decision**: Enhance `RentalListResponse` and `RentalDetailResponse` with payment summary fields: `TotalPaid` (decimal), `RentalRate` (decimal), and `OutstandingBalance` (decimal). `RentalDetailResponse` additionally includes a `Payments` list for per-rental payment history.

**Rationale**: The spec requires the My Rentals page to show payment status (total paid, outstanding) per rental (FR-011), and the rental detail page to show payment history (FR-013). Computing `OutstandingBalance = RentalRate - TotalPaid` on the server ensures consistency. Including `RentalRate` enables the frontend to pre-fill the "Return & Pay" form. The `Payments` list on the detail response uses a flat inline structure (amount + date + staffId) since it's one level deep.

**Alternatives considered**:
- Compute outstanding balance client-side → Rejected: duplicates business logic, violates clean architecture (frontend must not contain business logic per constitution).
- Separate endpoint for payment summary → Rejected: over-engineered for this use case; YAGNI.

## Decision 6: Payment List DTO Design

**Decision**: Create `PaymentListResponse` (flat, IDs only) and `PaymentDetailResponse` (flat with denormalized names) following the existing rental DTO pattern.

**Rationale**: Consistent with the existing `RentalListResponse`/`RentalDetailResponse` pattern. List responses are lean (IDs) for pagination performance. Detail responses inline one-level-deep related data as flat properties per constitution v1.9.0.

**Alternatives considered**:
- Single response type for both list and detail → Rejected: list would over-fetch, harming pagination performance with large datasets.

## Decision 7: Customer Payment Visibility

**Decision**: Customers see payments via the existing `GET /api/payments` endpoint with automatic `customerId` scoping (same pattern as rentals). The controller forces the customer's own ID, ignoring any client-provided customerId filter.

**Rationale**: Consistent with `RentalsController.GetRentals()` which already forces `customerId` for Customer-role users. Customers cannot see other customers' payments (FR-008). The controller resolves customerId from the JWT via UserManager (existing `GetCurrentUserCustomerId()` pattern).

**Alternatives considered**:
- Separate customer-only endpoint → Rejected: unnecessary duplication; role-based scoping in a single endpoint is the established pattern.

## Decision 8: New Dependencies

**Decision**: No new NuGet or npm packages required.

**Rationale**: All needed capabilities exist in current dependencies:
- Backend: FluentValidation, Ardalis.Result, EF Core — all already in use
- Frontend: React Query, Zod, Sonner, Shadcn UI, Lucide — all already in use
- The `DollarSign` icon from Lucide React will be used for the Payments nav item
