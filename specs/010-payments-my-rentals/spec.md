# Feature Specification: Payments & My Rentals Enhancement

**Feature Branch**: `010-payments-my-rentals`
**Created**: 2026-02-25
**Status**: Draft
**Input**: User description: "Implement the Payments system and enhance the My Rentals experience."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record Payment for a Rental (Priority: P1)

A staff member records a payment against a rental when a customer pays. The payment captures the rental it applies to, the amount, the staff member who processed it, and the date. The amount must be positive. The system pre-fills the amount from the film's rental rate when applicable, but staff can adjust it. Payments can be recorded at rental creation time (initial payment) or at any point during the rental lifecycle.

**Why this priority**: Payments are the core financial transaction of the rental business. Without the ability to record payments, no revenue tracking is possible. This is the foundational capability all other stories depend on.

**Independent Test**: Can be fully tested by creating a rental, then recording a payment against it, and verifying the payment is persisted with correct amount, date, rental link, and staff attribution.

**Acceptance Scenarios**:

1. **Given** a staff member is authenticated and a rental exists, **When** they submit a payment with a valid rental ID, positive amount, and their staff ID, **Then** the payment is recorded with the provided details and the current date (if no date specified).
2. **Given** a staff member submits a payment, **When** the amount is zero or negative, **Then** the system rejects the request with a validation error.
3. **Given** a staff member submits a payment, **When** the rental ID does not exist, **Then** the system returns a not-found error.
4. **Given** a staff member submits a payment, **When** multiple validation errors exist (e.g., invalid amount AND invalid rental ID), **Then** all errors are returned together in a single response.
5. **Given** a payment is submitted with no explicit payment date, **When** the system processes it, **Then** the payment date defaults to the current date/time.

---

### User Story 2 - View Payment History (Priority: P2)

Users can view payments filtered by customer, staff member, or rental. Administrators see all payments across the system. Staff members see payments from their store. Customers see only their own payments. Results are paginated for large datasets.

**Why this priority**: Visibility into payment history is essential for accountability, dispute resolution, and financial reconciliation. It directly enables the My Rentals enhancement by providing the data needed to show payment status.

**Independent Test**: Can be fully tested by creating several payments across different customers and staff, then querying with various filter combinations and verifying correct results and role-based filtering.

**Acceptance Scenarios**:

1. **Given** an administrator is authenticated, **When** they request the payment list without filters, **Then** all payments across all customers and staff are returned, paginated.
2. **Given** a staff member is authenticated, **When** they request the payment list, **Then** all payments for rentals at their store are returned (scoped by the staff member's store).
3. **Given** a customer is authenticated, **When** they request the payment list, **Then** only their own payments are returned.
4. **Given** any authenticated user, **When** they filter payments by rental ID, **Then** only payments for that specific rental are returned (subject to role-based visibility).
5. **Given** a customer, **When** they attempt to filter by another customer's ID, **Then** the system ignores the filter and returns only their own payments.
6. **Given** more payments exist than the page size, **When** a user requests page 2, **Then** the correct subset is returned with accurate total count and total pages.

---

### User Story 3 - Return Rental with Optional Payment (Priority: P3)

When a staff member processes a rental return, they can optionally include a payment in the same operation. If the rental has no prior payment, the system suggests the film's rental rate as the default amount. The return and payment are processed together as a single action.

**Why this priority**: Combining return and payment into one action reflects the real-world workflow at a rental counter and reduces the number of steps staff must take. It builds on P1 (payment recording) and the existing return functionality.

**Independent Test**: Can be fully tested by creating a rental with no payment, then returning it with a payment amount, and verifying both the return date is set and the payment is recorded.

**Acceptance Scenarios**:

1. **Given** a staff member is processing a return, **When** they include a valid payment amount, **Then** the rental is marked as returned AND a payment is recorded in a single operation.
2. **Given** a staff member is processing a return, **When** they do not include a payment amount, **Then** the rental is returned as before with no payment created (backward compatible).
3. **Given** a rental that already has a payment recorded, **When** a staff member returns it with another payment amount, **Then** both the return and the additional payment are recorded (multiple payments per rental are allowed).
4. **Given** a staff member includes a payment amount of zero or negative, **When** they attempt to return, **Then** the system rejects the request with a validation error and neither the return nor the payment is processed.
5. **Given** an active rental, **When** a staff member returns it with payment, **Then** the response includes the updated rental details showing the return date.

---

### User Story 4 - Enhanced My Rentals Experience (Priority: P4)

Customers viewing their rentals see a clear distinction between active and past rentals, along with payment status for each. Each rental shows the total amount paid and any outstanding balance (based on the film's rental rate). Staff and admin users see a "Return & Pay" action on active rentals, which opens a pre-filled form with the rental rate amount. Staff members also see a payments section in the navigation for managing all payment activity. Customers see payment information contextually within their rentals but cannot initiate returns or payments.

**Why this priority**: This is the user-facing enhancement that surfaces payment data in a meaningful way. It depends on P1-P3 being complete to have data to display and actions to perform.

**Independent Test**: Can be fully tested by viewing the My Rentals page with rentals in various states (active with payment, active without payment, returned with payment, returned without payment) and verifying the correct status, amounts, and available actions are displayed.

**Acceptance Scenarios**:

1. **Given** a customer has active rentals, **When** they view My Rentals, **Then** active rentals are shown with a visual indicator, the total paid, and any outstanding balance.
2. **Given** a customer has returned rentals, **When** they view My Rentals, **Then** past rentals are shown with return date and full payment summary.
3. **Given** an active rental with no payment, **When** the customer views it, **Then** the outstanding amount equals the film's rental rate.
4. **Given** a staff or admin user views an active rental, **When** they click "Return & Pay," **Then** a form opens with the amount pre-filled from the film's rental rate.
5. **Given** a staff or admin user completes the "Return & Pay" form, **When** they submit, **Then** the rental is returned and payment is recorded, and the rental card updates to reflect the new status.
6. **Given** a customer views an active rental, **When** they look for return or payment actions, **Then** no such actions are available (read-only payment status only).
7. **Given** a staff user is authenticated, **When** they view the navigation, **Then** a "Payments" section is visible for managing all payment activity.
8. **Given** a customer user is authenticated, **When** they view the navigation, **Then** payment information is shown contextually within their rentals (no separate payments section).

---

### User Story 5 - Payment History per Rental (Priority: P5)

When viewing a rental's details, users can see all payments associated with that rental. This provides a complete financial picture for each individual rental transaction.

**Why this priority**: Granular per-rental payment history supports dispute resolution and detailed financial tracking. It builds on P2's payment listing but scopes it to a single rental context.

**Independent Test**: Can be fully tested by creating a rental with multiple payments, then viewing the rental detail and verifying all associated payments are listed with correct amounts and dates.

**Acceptance Scenarios**:

1. **Given** a rental with one or more payments, **When** a user views the rental detail, **Then** all payments for that rental are displayed with amount and date.
2. **Given** a rental with no payments, **When** a user views the rental detail, **Then** a "No payments recorded" message is shown.
3. **Given** a rental with multiple payments, **When** a user views the rental detail, **Then** payments are listed in chronological order and a total is displayed.

---

### Edge Cases

- What happens when a payment is submitted for a rental that belongs to a different customer than specified? The system validates that the payment's customer matches the rental's customer and rejects mismatches.
- What happens when a staff member who is inactive attempts to process a payment? The system rejects the payment with an appropriate error.
- How does the system handle concurrent payment submissions for the same rental? Each payment is recorded independently; the system does not enforce a maximum total.
- What happens when the rental rate for a film is zero (free rental)? Payments are still allowed but not required; the outstanding balance shows as zero.
- What if a customer has no linked customer profile? Payment operations require a valid customer ID linked to the rental; the system returns an appropriate error.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow authenticated staff and admin users to record a payment against an existing rental, capturing the rental reference, amount, processing staff, and payment date.
- **FR-002**: System MUST reject payments with non-positive amounts, returning a validation error.
- **FR-003**: System MUST validate that the referenced rental exists before accepting a payment.
- **FR-004**: System MUST validate that the payment's customer matches the rental's customer.
- **FR-005**: System MUST default the payment date to the current date/time when not explicitly provided.
- **FR-006**: System MUST aggregate all validation and business-rule errors into a single response rather than returning on first failure.
- **FR-007**: System MUST support listing payments with optional filters for customer, staff member, and rental, with paginated results.
- **FR-008**: System MUST enforce role-based payment visibility: administrators see all payments system-wide, staff see all payments at their store (scoped via Rental → Inventory → Store), customers see only their own payments.
- **FR-009**: System MUST allow the rental return operation to optionally include a payment amount, processing both the return and payment as a combined action.
- **FR-010**: System MUST maintain backward compatibility for the rental return operation — omitting the payment amount must behave identically to the current return behavior.
- **FR-011**: System MUST display active and past rentals on the My Rentals page with payment status (total paid, outstanding balance based on rental rate).
- **FR-012**: System MUST provide a "Return & Pay" action for active rentals (visible to staff and admin only) that pre-fills the payment amount from the film's rental rate.
- **FR-013**: System MUST display payment history for each rental when viewing rental details.
- **FR-014**: System MUST show a "Payments" navigation entry for staff and admin users, while customers see payment information contextually within their rentals.
- **FR-015**: System MUST allow multiple payments per rental (no restriction on payment count or total amount per rental).

### Key Entities

- **Payment**: A financial transaction recorded against a rental. Captures the customer who owes, the staff member who processed it, the rental it applies to, the monetary amount, and the date of payment. A rental can have zero or many payments.
- **Rental** (existing, enhanced): Now surfaces payment summary information — total paid and outstanding balance calculated from the associated film's rental rate minus sum of payments.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Staff can record a payment against a rental in under 30 seconds from the payment form.
- **SC-002**: The "Return & Pay" combined action completes in a single user interaction rather than requiring separate return and payment steps.
- **SC-003**: Customers can see the payment status (paid, outstanding) of every rental on their My Rentals page without navigating to individual rental details.
- **SC-004**: Payment history for a rental is accessible within one click from the rental detail view.
- **SC-005**: Role-based filtering ensures users never see payment data outside their authorization scope (100% of unauthorized access attempts are blocked).
- **SC-006**: All validation errors for a payment submission are surfaced to the user in a single response, reducing back-and-forth correction cycles.
- **SC-007**: The existing rental return workflow continues to function identically when no payment information is provided (zero regressions).

## Clarifications

### Session 2026-02-25

- Q: Should staff see only payments they personally processed, all payments system-wide, or all payments at their store? → A: Staff see all payments at their store (scoped by store_id via Rental → Inventory → Store).
- Q: Can customers initiate "Return & Pay" from My Rentals? → A: No. "Return & Pay" is staff/admin only. The existing schema requires a real staff_id on payments (non-nullable), and customer self-service returns would require schema changes or sentinel records. Customers see payment status and history on My Rentals but cannot initiate returns or payments.

## Assumptions

- The existing Payment entity in the database (with fields: payment_id, customer_id, staff_id, rental_id, amount, payment_date) is the correct schema for this feature — no schema migration is needed.
- The film's rental_rate field is the correct basis for calculating outstanding balances and pre-filling payment amounts.
- "Outstanding balance" is calculated as rental_rate minus sum of payment amounts for a given rental. Negative balances (overpayment) are displayed but do not trigger refunds.
- Multiple payments per rental are allowed by design (e.g., partial payments, deposits).
- The "Return & Pay" action in the frontend initiates the enhanced return endpoint; it does not create a separate payment request.
- Customer-role users cannot initiate returns or record payments. They have read-only access to payment status and history on their rentals. "Return & Pay" is exclusively a staff/admin action.
- Staff-role users in the navigation see a dedicated "Payments" page; customers do not see a separate payments page but see payment info inline on their rentals.
