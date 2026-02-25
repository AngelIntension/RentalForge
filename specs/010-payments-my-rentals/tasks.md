# Tasks: Payments & My Rentals Enhancement

**Input**: Design documents from `/specs/010-payments-my-rentals/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: TDD is NON-NEGOTIABLE per constitution v1.9.0. All production code follows red-green-refactor.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create all new DTO types, enhance existing DTOs, add ApplicationUser.StaffId migration, and prepare seeder

- [ ] T001 [P] Create CreatePaymentRequest record in src/RentalForge.Api/Models/CreatePaymentRequest.cs (fields: RentalId int, Amount decimal, PaymentDate DateTime?, StaffId int; all init-only)
- [ ] T002 [P] Create ReturnRentalRequest record in src/RentalForge.Api/Models/ReturnRentalRequest.cs (fields: Amount decimal?, StaffId int?; all init-only)
- [ ] T003 [P] Create PaymentListResponse record in src/RentalForge.Api/Models/PaymentListResponse.cs (fields: Id, RentalId, CustomerId, StaffId, Amount, PaymentDate; per data-model.md)
- [ ] T004 [P] Create PaymentDetailResponse record in src/RentalForge.Api/Models/PaymentDetailResponse.cs (fields: Id, RentalId, CustomerId, CustomerFirstName, CustomerLastName, StaffId, StaffFirstName, StaffLastName, Amount, PaymentDate, FilmTitle; per data-model.md)
- [ ] T005 [P] Create RentalPaymentItem record in src/RentalForge.Api/Models/RentalPaymentItem.cs (fields: Id int, Amount decimal, PaymentDate DateTime, StaffId int)
- [ ] T006 Enhance RentalListResponse record in src/RentalForge.Api/Models/RentalListResponse.cs — append TotalPaid decimal = 0, RentalRate decimal = 0, OutstandingBalance decimal = 0 as defaulted positional parameters (existing callers continue to compile)
- [ ] T007 Enhance RentalDetailResponse record in src/RentalForge.Api/Models/RentalDetailResponse.cs — append TotalPaid decimal = 0, RentalRate decimal = 0, OutstandingBalance decimal = 0 as defaulted positional parameters; add Payments as a separate init-only property: `public IReadOnlyList<RentalPaymentItem> Payments { get; init; } = [];` (existing callers continue to compile; queries return defaults until T040-T041 populate them)
- [ ] T008 [P] Add nullable StaffId to ApplicationUser in src/RentalForge.Api/Data/Entities/ApplicationUser.cs — add `public int? StaffId { get; set; }` and `public Staff? Staff { get; set; }` navigation property, mirroring the existing CustomerId/Customer pattern
- [ ] T009 Create EF Core migration for ApplicationUser.StaffId — run `dotnet ef migrations add AddStaffIdToApplicationUser --project src/RentalForge.Api`; verify migration adds nullable smallint column, FK constraint to staff table, and index on AspNetUsers.StaffId
- [ ] T010 Update DevDataSeeder in src/RentalForge.Api/Data/Seeding/DevDataSeeder.cs — in SeedAuthUsersAsync(), set StaffId on staff and admin users (e.g., staff@rentalforge.dev → staff_id 1, admin@rentalforge.dev → staff_id 1); update the users array tuple to include StaffId

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Test helpers, validators, and frontend type infrastructure that MUST be complete before user stories

- [ ] T011 Create PaymentTestHelper in tests/RentalForge.Api.Tests/Helpers/PaymentTestHelper.cs — seed methods for payment test data using 9000+ ID range, matching existing RentalTestHelper pattern (SeedPaymentAsync, SeedMultiplePaymentsAsync, SeedPaymentForRentalAsync)
- [ ] T012 [P] Create CreatePaymentValidator in src/RentalForge.Api/Validators/CreatePaymentValidator.cs — RentalId > 0, Amount > 0, StaffId > 0 (FluentValidation rules matching existing validator pattern)
- [ ] T013 [P] Create ReturnRentalValidator in src/RentalForge.Api/Validators/ReturnRentalValidator.cs — Amount > 0 when provided, StaffId > 0 and required when Amount is provided (conditional rules using .When())
- [ ] T014 [P] Create payment TypeScript types in src/RentalForge.Web/src/types/payment.ts — PaymentListItem, PaymentDetail, CreatePaymentRequest, PaymentSearchParams interfaces per contracts/payments-api.md
- [ ] T015 [P] Update rental TypeScript types in src/RentalForge.Web/src/types/rental.ts — add totalPaid, rentalRate, outstandingBalance to RentalListItem and RentalDetail; add RentalPaymentItem interface; add ReturnRentalRequest interface with optional amount and staffId
- [ ] T016 [P] Add payment test fixtures in src/RentalForge.Web/src/test/fixtures/data.ts — samplePaymentListItem, samplePaymentListItems (array of 3), samplePaymentDetail following existing fixture patterns
- [ ] T017 [P] Update rental test fixtures in src/RentalForge.Web/src/test/fixtures/data.ts — add totalPaid, rentalRate, outstandingBalance to existing sampleRentalListItem/sampleRentalDetail; add sampleRentalPaymentItems; update sampleReturnedRentalDetail
- [ ] T018 Add payment MSW handlers in src/RentalForge.Web/src/test/mocks/handlers.ts — regex patterns for GET /api/payments (list with paginate helper), POST /api/payments (return 201 with samplePaymentDetail); update PUT /api/rentals/{id}/return handler to accept optional body

**Checkpoint**: All shared types, DTOs, validators, test infrastructure, and ApplicationUser migration complete. User story implementation can now begin.

---

## Phase 3: User Story 1 — Record Payment for a Rental (Priority: P1) MVP

**Goal**: Staff/admin can create a payment against an existing rental via POST /api/payments with full validation and error aggregation.

**Independent Test**: Create a rental, POST a payment with valid data, verify 201 with PaymentDetailResponse. POST with invalid data, verify 400 with aggregated errors.

### Tests for User Story 1 (TDD — write and verify RED first)

- [ ] T019 [P] [US1] Write failing unit tests for PaymentService.CreatePaymentAsync in tests/RentalForge.Api.Tests/PaymentServiceTests.cs — test cases: successful creation returns Created with PaymentDetailResponse, invalid amount returns Invalid, non-existent rental returns Invalid, inactive staff returns Invalid, aggregated errors for multiple failures, payment date defaults to UtcNow when null, two payments against same rental both persist successfully (FR-015). Use AutoFixture for test data, FluentAssertions for assertions, mock DvdrentalContext with Testcontainers.
- [ ] T020 [P] [US1] Write failing integration tests for POST /api/payments in tests/RentalForge.Api.Tests/PaymentEndpointTests.cs — test cases: 201 Created with valid data and Location header, 400 with aggregated validation errors, 401 for unauthenticated, 403 for Customer role. Use TestWebAppFactory, AuthTestHelper.CreateAuthenticatedClient for Staff/Admin/Customer roles.

### Implementation for User Story 1

- [ ] T021 [US1] Create IPaymentService interface in src/RentalForge.Api/Services/IPaymentService.cs — CreatePaymentAsync(CreatePaymentRequest request) returning Task\<Result\<PaymentDetailResponse\>\>
- [ ] T022 [US1] Implement PaymentService.CreatePaymentAsync in src/RentalForge.Api/Services/PaymentService.cs — primary constructor injects DvdrentalContext, ILogger, IValidator\<CreatePaymentRequest\>; FluentValidation via .AsErrors() + service-level checks (rental exists, staff exists+active, customer derived from rental); create Payment entity, save, return enriched PaymentDetailResponse via projected query; use structured logging
- [ ] T023 [US1] Register IPaymentService in src/RentalForge.Api/Program.cs — add builder.Services.AddScoped\<IPaymentService, PaymentService\>() following existing service registration pattern
- [ ] T024 [US1] Create PaymentsController with POST endpoint in src/RentalForge.Api/Controllers/PaymentsController.cs — [Route("api/payments")], [Authorize], constructor injects IPaymentService; POST [Authorize(Roles = "Staff,Admin")] returns CreatedAtAction on success, InvalidResult on validation failure; copy InvalidResult helper pattern from RentalsController
- [ ] T025 [US1] Verify all US1 tests pass — run dotnet test --filter "PaymentService" and dotnet test --filter "PaymentEndpoint" to confirm GREEN

**Checkpoint**: POST /api/payments fully functional with TDD validation. Payments can be recorded against rentals.

---

## Phase 4: User Story 2 — View Payment History (Priority: P2)

**Goal**: Paginated payment listing with role-based visibility — Admin sees all, Staff sees store-scoped, Customer sees own only.

**Independent Test**: Create payments across different stores/customers, query as Admin (see all), Staff (see store-only), Customer (see own-only), verify correct filtering and pagination.

### Tests for User Story 2 (TDD — write and verify RED first)

- [ ] T026 [P] [US2] Write failing unit tests for PaymentService.GetPaymentsAsync in tests/RentalForge.Api.Tests/PaymentServiceTests.cs — test cases: returns paginated results, filters by customerId, filters by staffId, filters by rentalId, correct totalCount/totalPages, empty results return empty page. Use Testcontainers with seeded multi-store/multi-customer payment data.
- [ ] T027 [P] [US2] Write failing integration tests for GET /api/payments role-based filtering in tests/RentalForge.Api.Tests/PaymentEndpointTests.cs — test cases: Admin sees all payments, Staff sees only store-scoped payments (seed payments across 2 stores), Customer sees only own payments, Customer cannot filter by other customerId, pagination with page/pageSize params, 401 for unauthenticated, Customer without linked customer profile gets appropriate error (EC5).

### Implementation for User Story 2

- [ ] T028 [US2] Add GetPaymentsAsync to IPaymentService in src/RentalForge.Api/Services/IPaymentService.cs — signature: Task\<PagedResponse\<PaymentListResponse\>\> GetPaymentsAsync(int? customerId, int? staffId, int? rentalId, int? storeId, int page, int pageSize)
- [ ] T029 [US2] Implement PaymentService.GetPaymentsAsync in src/RentalForge.Api/Services/PaymentService.cs — LINQ query on db.Payments with optional filters (customerId, staffId, rentalId, storeId via r.Rental.Inventory.StoreId), order by PaymentDate descending then PaymentId descending, project to PaymentListResponse, return PagedResponse
- [ ] T030 [US2] Add GET endpoint and role-based helper to PaymentsController in src/RentalForge.Api/Controllers/PaymentsController.cs — GET [Authorize] with query params (customerId, staffId, rentalId, page, pageSize); add GetCurrentUserCustomerId() helper (same pattern as RentalsController); add GetCurrentUserStoreId() helper that resolves Staff.StoreId via ApplicationUser.StaffId → Staff.StoreId (using UserManager to get ApplicationUser, then querying Staff by user.StaffId); for Customer role force customerId, for Staff role force storeId, for Admin no restrictions
- [ ] T031 [US2] Verify all US2 tests pass — run full PaymentService and PaymentEndpoint tests to confirm GREEN

**Checkpoint**: GET /api/payments fully functional with role-based store-scoped and customer-scoped filtering.

---

## Phase 5: User Story 3 — Return Rental with Optional Payment (Priority: P3)

**Goal**: Enhanced PUT /api/rentals/{id}/return accepts optional payment amount+staffId, creating a payment in the same operation while maintaining backward compatibility.

**Independent Test**: Return a rental without payment (backward compatible), return a rental with payment (verify both return date set and payment created), return with invalid payment amount (verify rejected).

### Tests for User Story 3 (TDD — write and verify RED first)

- [ ] T032 [P] [US3] Write failing unit tests for enhanced ReturnRentalAsync in tests/RentalForge.Api.Tests/RentalServiceTests.cs — test cases: return without request (backward compatible, no payment created), return with valid amount+staffId (rental returned AND payment created), return with invalid amount (rejected, no return processed), return with amount but missing staffId (rejected), return with inactive staff (rejected), already-returned rental still rejected
- [ ] T033 [P] [US3] Write failing integration tests for enhanced PUT /api/rentals/{id}/return in tests/RentalForge.Api.Tests/RentalEndpointTests.cs — test cases: PUT with no body returns 200 (backward compatible), PUT with payment body returns 200 with rental detail, PUT with invalid amount returns 400, verify payment record exists in DB after return+pay

### Implementation for User Story 3

- [ ] T034 [US3] Update IRentalService.ReturnRentalAsync signature in src/RentalForge.Api/Services/IRentalService.cs — change from ReturnRentalAsync(int id) to ReturnRentalAsync(int id, ReturnRentalRequest? request = null)
- [ ] T035 [US3] Implement enhanced ReturnRentalAsync in src/RentalForge.Api/Services/RentalService.cs — inject IValidator\<ReturnRentalRequest\> in constructor; when request?.Amount is provided: validate via ReturnRentalValidator, check staff exists+active, derive customerId from rental, create Payment entity, save all in single SaveChangesAsync; when request is null or Amount is null: existing behavior unchanged
- [ ] T036 [US3] Update RentalsController.ReturnRental in src/RentalForge.Api/Controllers/RentalsController.cs — change signature to accept [FromBody] ReturnRentalRequest? request = null, pass to service; existing result status mapping unchanged
- [ ] T037 [US3] Verify all US3 tests pass AND verify existing rental return tests still pass (backward compatibility) — run dotnet test --filter "RentalService" and dotnet test --filter "RentalEndpoint"

**Checkpoint**: Enhanced return endpoint works with and without payment. All existing rental tests still pass.

---

## Phase 6: User Story 4 — Enhanced My Rentals Experience (Priority: P4)

**Goal**: Backend rental queries return payment summary (totalPaid, rentalRate, outstandingBalance). Frontend shows payment status on rental cards, "Return & Pay" modal for staff/admin, Payments nav item and page for staff/admin.

**Independent Test**: View My Rentals with rentals in various payment states — verify correct amounts displayed. Staff clicks "Return & Pay" — verify modal pre-fills amount, submission works. Payments nav visible for staff, hidden for customer.

### Backend: Enhanced Rental Queries

- [ ] T038 [P] [US4] Write failing tests for enhanced GetRentalsAsync in tests/RentalForge.Api.Tests/RentalServiceTests.cs — test cases: RentalListResponse includes totalPaid (sum of payments), rentalRate (from film), outstandingBalance (rate - paid); rental with no payments shows totalPaid=0 and outstandingBalance=rentalRate; rental with multiple payments shows correct sum; rental with zero rental rate shows rentalRate=0, outstandingBalance=0 (EC4)
- [ ] T039 [P] [US4] Write failing tests for enhanced GetRentalByIdAsync in tests/RentalForge.Api.Tests/RentalServiceTests.cs — test cases: RentalDetailResponse includes payment summary fields AND payments list; payments ordered chronologically; rental with no payments returns empty payments list
- [ ] T040 [US4] Update RentalService.GetRentalsAsync in src/RentalForge.Api/Services/RentalService.cs — enhance LINQ projection to include TotalPaid = r.Payments.Sum(p => p.Amount), RentalRate = r.Inventory.Film.RentalRate, OutstandingBalance = r.Inventory.Film.RentalRate - r.Payments.Sum(p => p.Amount)
- [ ] T041 [US4] Update RentalService.GetRentalByIdAsync in src/RentalForge.Api/Services/RentalService.cs — enhance LINQ projection to include payment summary fields plus Payments = r.Payments.OrderBy(p => p.PaymentDate).Select(p => new RentalPaymentItem(...)).ToList()
- [ ] T042 [US4] Fix existing rental tests in tests/RentalForge.Api.Tests/ — update all existing rental test assertions to account for new TotalPaid, RentalRate, OutstandingBalance, Payments fields on response DTOs
- [ ] T043 [US4] Verify all backend tests pass — run dotnet test to confirm GREEN across all test files

### Frontend: Hooks and Validators

- [ ] T044 [P] [US4] Update useReturnRental hook in src/RentalForge.Web/src/hooks/use-rentals.ts — change mutation to accept { id: number, request?: ReturnRentalRequest } and pass request body to api.put; maintain backward compatibility when request is undefined
- [ ] T045 [P] [US4] Create usePayments hook in src/RentalForge.Web/src/hooks/use-payments.ts — useInfinitePayments(params: PaymentSearchParams) and useCreatePayment() following existing hook patterns; query key ['payments']; invalidate on create success
- [ ] T046 [P] [US4] Add payment Zod schemas in src/RentalForge.Web/src/lib/validators.ts — createPaymentSchema (rentalId, amount, staffId as coerced positive numbers), returnPaySchema (amount as coerced positive number, staffId as coerced positive number)

### Frontend: Components

- [ ] T047 [P] [US4] Create ReturnPayModal component in src/RentalForge.Web/src/components/rentals/return-pay-modal.tsx — Dialog/Sheet with form for amount (pre-filled from rentalRate prop) and staffId; Zod validation via returnPaySchema; onSubmit calls parent handler; onCancel closes modal
- [ ] T048 [P] [US4] Create payment-card component in src/RentalForge.Web/src/components/payments/payment-card.tsx — displays PaymentListItem with amount, date, rental ID, customer ID, staff ID; follows rental-card pattern with Link wrapper
- [ ] T049 [US4] Update rental-card component in src/RentalForge.Web/src/components/rentals/rental-card.tsx — show totalPaid and outstandingBalance; replace simple "Return" button with "Return & Pay" button for staff/admin (hidden for customers using useAuth role check); color-code outstanding balance (green if paid, amber if outstanding)

### Frontend: Pages and Navigation

- [ ] T050 [US4] Create payments-list page in src/RentalForge.Web/src/pages/payments-list.tsx — useInfinitePayments with infinite scroll, PaymentCard for each item, LoadMore component; follows rentals-list pattern
- [ ] T051 [US4] Update rentals-list page in src/RentalForge.Web/src/pages/rentals-list.tsx — integrate ReturnPayModal; on "Return & Pay" click open modal with rentalRate pre-filled; on submit call returnRental.mutate with id and request body; toast success/error
- [ ] T052 [P] [US4] Add Payments nav item to both src/RentalForge.Web/src/components/layout/bottom-nav.tsx and src/RentalForge.Web/src/components/layout/sidebar-nav.tsx — { to: '/payments', icon: DollarSign, label: 'Payments', roles: ['Staff', 'Admin'] }; import DollarSign from lucide-react
- [ ] T053 [US4] Add /payments route in src/RentalForge.Web/src/app/routes.tsx — { path: 'payments', element: \<ProtectedRoute allowedRoles={['Staff', 'Admin']}\>\<PaymentsList /\>\</ProtectedRoute\> } inside the authenticated layout children

### Frontend: Tests

- [ ] T054 [P] [US4] Write tests for enhanced rental-card in src/RentalForge.Web/src/test/ — verify payment status displayed (totalPaid, outstandingBalance), "Return & Pay" button visible for Staff role (mock useAuth), button hidden for Customer role, Return & Pay button click opens modal
- [ ] T055 [P] [US4] Write tests for ReturnPayModal in src/RentalForge.Web/src/test/ — verify amount pre-filled from rentalRate, validation rejects zero/negative amount, submit calls onSubmit with correct data, cancel closes modal
- [ ] T056 [P] [US4] Write tests for payments-list page in src/RentalForge.Web/src/test/ — verify renders payment cards from MSW handler, shows loading state, shows empty state when no payments
- [ ] T057 [US4] Write tests for updated rentals-list page in src/RentalForge.Web/src/test/ — verify Return & Pay flow: click button → modal opens → submit → toast success → modal closes
- [ ] T058 [US4] Verify all frontend tests pass — run npm run test from src/RentalForge.Web

**Checkpoint**: Enhanced My Rentals with payment status, Return & Pay modal, Payments page, and nav items. Staff/admin can manage payments end-to-end.

---

## Phase 7: User Story 5 — Payment History per Rental (Priority: P5)

**Goal**: Rental detail page shows all payments for that rental with amounts, dates, and totals.

**Independent Test**: View a rental with multiple payments — verify all payments listed chronologically with total. View a rental with no payments — verify "No payments recorded" message.

- [ ] T059 [US5] Update rental-detail component in src/RentalForge.Web/src/components/rentals/rental-detail.tsx — add "Payments" section after existing detail rows; if rental.payments is empty show "No payments recorded" message; if payments exist show list with amount, date, staffId per item and a total row; use Separator between detail section and payments section
- [ ] T060 [US5] Write tests for updated rental-detail component in src/RentalForge.Web/src/test/ — verify payment history displayed when rental has payments (use updated sampleRentalDetail fixture with payments); verify "No payments recorded" shown when payments array is empty; verify total is calculated correctly; verify payments are in chronological order
- [ ] T061 [US5] Verify all frontend tests pass — run npm run test from src/RentalForge.Web

**Checkpoint**: Rental detail page shows complete payment history. All user stories functional and tested.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, build verification, cross-story integration checks

- [ ] T062 Run full backend test suite — dotnet test (expect all tests pass including new payment tests and updated rental tests)
- [ ] T063 Run full frontend test suite — npm run test from src/RentalForge.Web (expect all tests pass)
- [ ] T064 Run frontend build — npm run build from src/RentalForge.Web (TypeScript compile + Vite production build must succeed)
- [ ] T065 Run frontend typecheck and lint — npm run typecheck && npm run lint from src/RentalForge.Web
- [ ] T066 Run backend build — dotnet build (must succeed with zero warnings)
- [ ] T067 Verify backward compatibility — confirm existing rental endpoints return enhanced DTOs with payment summary fields without breaking existing consumers; confirm PUT /api/rentals/{id}/return with no body behaves identically to pre-feature behavior

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (DTOs exist before validators reference them)
- **US1 (Phase 3)**: Depends on Phase 2 — No dependencies on other stories
- **US2 (Phase 4)**: Depends on Phase 2 — No dependencies on US1 (independent service methods)
- **US3 (Phase 5)**: Depends on Phase 2 — No dependencies on US1/US2 (modifies RentalService, not PaymentService)
- **US4 (Phase 6)**: Depends on Phase 2 (backend enhancement) + Phase 3/4/5 recommended complete (frontend needs all API endpoints available)
- **US5 (Phase 7)**: Depends on US4 backend tasks (T038-T043) for enhanced RentalDetailResponse
- **Polish (Phase 8)**: Depends on all user stories complete

### User Story Dependencies

- **US1 (P1)**: Independent after Phase 2
- **US2 (P2)**: Independent after Phase 2 — can run parallel with US1
- **US3 (P3)**: Independent after Phase 2 — can run parallel with US1, US2
- **US4 (P4)**: Backend tasks (T038-T043) independent after Phase 2; frontend tasks need US1+US2+US3 backend endpoints available
- **US5 (P5)**: Needs US4 backend tasks complete (enhanced detail response)

### Within Each User Story

- Tests MUST be written and FAIL before implementation (TDD red-green-refactor)
- DTOs/models before services
- Services before controllers/endpoints
- Backend before frontend (within same story)
- Core implementation before integration tests
- Story complete before moving to next priority

### Parallel Opportunities

- Phase 1: T001-T005, T008 all [P] (separate new files)
- Phase 2: T012-T017 all [P] (separate validator/type/fixture files)
- US1: T019-T020 [P] (unit tests and integration tests in separate files)
- US2: T026-T027 [P] (unit tests and integration tests in separate files)
- US3: T032-T033 [P] (unit tests and integration tests in separate files)
- US4: T038-T039 [P] (different test methods), T044-T046 [P] (different frontend files), T047-T048 [P] (different component files), T052 [P] (both nav files), T054-T056 [P] (different test files)
- US1, US2, US3 backend stories can all run in parallel after Phase 2

---

## Parallel Example: User Story 1

```bash
# Launch both test files in parallel (RED phase):
Task: "Write failing unit tests for PaymentService.CreatePaymentAsync in tests/PaymentServiceTests.cs"
Task: "Write failing integration tests for POST /api/payments in tests/PaymentEndpointTests.cs"

# Then implement sequentially (GREEN phase):
Task: "Create IPaymentService interface"
Task: "Implement PaymentService.CreatePaymentAsync"
Task: "Register IPaymentService in Program.cs"
Task: "Create PaymentsController with POST endpoint"
Task: "Verify all US1 tests pass"
```

## Parallel Example: User Story 4 Frontend

```bash
# Launch all independent frontend tasks in parallel:
Task: "Update useReturnRental hook in use-rentals.ts"
Task: "Create usePayments hook in use-payments.ts"
Task: "Add payment Zod schemas in validators.ts"

# Launch component creation in parallel:
Task: "Create ReturnPayModal component"
Task: "Create payment-card component"

# Launch test files in parallel:
Task: "Write tests for enhanced rental-card"
Task: "Write tests for ReturnPayModal"
Task: "Write tests for payments-list page"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T010)
2. Complete Phase 2: Foundational (T011-T018)
3. Complete Phase 3: User Story 1 (T019-T025)
4. **STOP and VALIDATE**: POST /api/payments works end-to-end with TDD coverage
5. Payments can be recorded — core financial transaction operational

### Incremental Delivery

1. Setup + Foundational → Infrastructure ready
2. US1 → Record payments (MVP!)
3. US2 → View/filter payments with role-based access
4. US3 → Return + pay in single action
5. US4 → Full frontend experience (cards, modals, pages, nav)
6. US5 → Per-rental payment history on detail page
7. Polish → Final validation

### Sequential Recommendation (Single Developer)

Execute US1 → US2 → US3 → US4 → US5 sequentially. Backend stories first (US1-3), then frontend stories (US4-5). This ensures all API endpoints exist before building the frontend that consumes them.

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- TDD is NON-NEGOTIABLE — every production task has corresponding test tasks that must RED first
- AutoFixture for anonymous test data, FluentAssertions for assertions
- Payment amount max 999.99 (numeric(5,2) DB constraint)
- StaffId from request body, not JWT (per research decision 2)
- MSW handlers must use regex patterns (per project convention)
- Zod imports from 'zod/v4' (not 'zod')
- Test ID range 9000+ to avoid collisions with reference data
- Commit after each task or logical group
