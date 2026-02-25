# Tasks: Authentication System

**Input**: Design documents from `/specs/009-auth-system/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/auth-api.md, quickstart.md

**Tests**: TDD is NON-NEGOTIABLE per constitution v1.9.0. Tests are written first (red), implementation follows (green), then refactor.

**Organization**: Tasks grouped by user story. Each story is independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Paths relative to repository root

---

## Phase 1: Setup

**Purpose**: Add dependencies and configure project for authentication

- [X] T001 Add NuGet packages: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.3 and `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.3 to `src/RentalForge.Api/RentalForge.Api.csproj`
- [X] T002 Add JWT configuration placeholder keys to `src/RentalForge.Api/appsettings.json` (empty values — actual secrets via `dotnet user-secrets`): `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenExpirationMinutes`, `Jwt:RefreshTokenExpirationDays`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core entities, DbContext, Identity/JWT infrastructure, test factory — MUST be complete before any user story

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 [P] Create `ApplicationUser` entity extending `IdentityUser` with `CustomerId` (int?, nullable FK to `Customer`) and `CreatedAt` (DateTime) in `src/RentalForge.Api/Data/Entities/ApplicationUser.cs`
- [X] T004 [P] Create `RefreshToken` entity with `Id` (Guid PK), `Token` (string, unique), `Family` (string), `UserId` (string FK), `ExpiresAt`, `IsUsed`, `RevokedAt`, `CreatedAt`, `xmin` (uint, EF Core concurrency token mapped to PostgreSQL system column) in `src/RentalForge.Api/Data/Entities/RefreshToken.cs`
- [X] T005 Modify `DvdrentalContext` to inherit from `IdentityDbContext<ApplicationUser, IdentityRole, string>` in `src/RentalForge.Api/Data/DvdrentalContext.cs`: call `base.OnModelCreating()`, configure all Identity tables to `identity` schema with snake_case names, configure `RefreshToken` in `identity.refresh_tokens`, configure `ApplicationUser.CustomerId` FK to `Customer` with 1:0..1 relationship, seed three roles (Admin, Staff, Customer) via `HasData()`, add `DbSet<RefreshToken>`. Also add `ApplicationUser? AuthUser` inverse navigation property to `src/RentalForge.Api/Data/Entities/Customer.cs` and configure the relationship in OnModelCreating
- [X] T006 Create all auth request/response DTOs as records in `src/RentalForge.Api/Models/Auth/`: `RegisterRequest` (Email, Password, Role?), `LoginRequest` (Email, Password), `RefreshRequest` (RefreshToken), `LogoutRequest` (RefreshToken), `AuthResponse` (Token, RefreshToken, User), `RefreshResponse` (Token, RefreshToken), `UserDto` (Id, Email, Role, CustomerId?, CreatedAt)
- [X] T007 Create `IAuthService` interface in `src/RentalForge.Api/Services/IAuthService.cs` with methods: `RegisterAsync(RegisterRequest, ClaimsPrincipal?)` → `Result<AuthResponse>`, `LoginAsync(LoginRequest)` → `Result<AuthResponse>`, `RefreshAsync(RefreshRequest)` → `Result<RefreshResponse>`, `LogoutAsync(LogoutRequest, string userId)` → `Result`, `GetMeAsync(string userId)` → `Result<UserDto>`
- [X] T008 Configure Identity, JWT bearer auth, and authorization in `src/RentalForge.Api/Program.cs`: register `AddIdentity<ApplicationUser, IdentityRole>` with password policy + `AddEntityFrameworkStores<DvdrentalContext>`, register `AddAuthentication` with `JwtBearerDefaults` + `AddJwtBearer` reading from `Jwt:*` config, register `AddAuthorization`, register `IAuthService`/`AuthService` scoped, add `UseAuthentication()` and `UseAuthorization()` before `MapControllers()`, register FluentValidation validators for auth requests
- [X] T009 Update `TestWebAppFactory` in `tests/RentalForge.Api.Tests/Infrastructure/TestWebAppFactory.cs`: configure Identity in test container, add in-memory JWT config (`Jwt:Key`, etc.), ensure `EnsureCreatedAsync` creates identity schema tables
- [X] T010 Create `AuthTestHelper` in `tests/RentalForge.Api.Tests/Infrastructure/AuthTestHelper.cs`: helper methods to create test users with `UserManager<ApplicationUser>`, assign roles, generate valid JWT tokens for authenticated test requests, create `HttpClient` with auth header

**Checkpoint**: Identity tables created in test container, JWT middleware active, auth infrastructure ready for story implementation

---

## Phase 3: User Story 1 — Account Registration (Priority: P1)

**Goal**: Visitors can register with email + password, get back tokens, and immediately use the app as a Customer. Admins can register users with elevated roles.

**Independent Test**: Register via POST /api/auth/register → receive 201 with JWT + refresh token + user info. Verify token is valid. Verify duplicate email returns 400. Verify password validation returns aggregated errors.

### Tests (TDD Red)

- [X] T011 [P] [US1] Write `RegisterRequestValidator` unit tests in `tests/RentalForge.Api.Tests/Unit/RegisterRequestValidatorTests.cs`: test email required/format, password required/min-length/uppercase/lowercase/digit/special-char, role defaults to Customer, all errors aggregated. Use AutoFixture for anonymous data.
- [X] T012 [P] [US1] Write register endpoint integration tests in `tests/RentalForge.Api.Tests/Integration/AuthEndpointTests.cs`: test successful registration returns 201 with token/refreshToken/user, duplicate email returns 400, invalid password returns 400 with all errors, elevated role without Admin returns 403, elevated role with Admin token returns 201 with assigned role

### Implementation (TDD Green)

- [X] T013 [US1] Implement `RegisterRequestValidator` in `src/RentalForge.Api/Validators/RegisterRequestValidator.cs`: validate Email (required, email format), Password (required, min 8, has upper/lower/digit/special), Role (when present, must be valid role name)
- [X] T014 [US1] Implement `AuthService.RegisterAsync` in `src/RentalForge.Api/Services/AuthService.cs`: inject `ILogger<AuthService>`, validate with FluentValidation + `.AsErrors()`, check role elevation authorization (non-Admin requesting Staff/Admin → Result.Forbidden), create user via `UserManager`, assign role, generate JWT (using `JsonWebTokenHandler`) with sub/email/role/iat/exp/jti claims, create RefreshToken with new family, return `AuthResponse`. Log: successful registration (email, role), failed registration (validation), forbidden role elevation attempt
- [X] T015 [US1] Implement `AuthController.Register` in `src/RentalForge.Api/Controllers/AuthController.cs`: `[HttpPost("register")]` `[AllowAnonymous]`, delegate to `IAuthService.RegisterAsync`, result.Status switch (Created → CreatedAtAction to /me, Invalid → InvalidResult, Forbidden → Forbid, Error → 500). Add `[ProducesResponseType<AuthResponse>(201)]`, `[ProducesResponseType(400)]`, `[ProducesResponseType(403)]`, `[ProducesResponseType(429)]` and XML doc summary
- [X] T016 [P] [US1] Create frontend auth type definitions in `src/RentalForge.Web/src/types/auth.ts`: `AuthResponse`, `RefreshResponse`, `UserDto`, `LoginRequest`, `RegisterRequest`, `RefreshRequest`, `LogoutRequest`
- [X] T017 [P] [US1] Add Zod register schema in `src/RentalForge.Web/src/lib/validators.ts`: `registerSchema` with email (email format), password (min 8, regex for strength), confirmPassword (must match password — frontend-only UX field, not sent to API)
- [X] T018 [US1] Write register page component tests in `src/RentalForge.Web/src/pages/register.test.tsx`: renders form fields, shows validation errors on invalid submit, calls API on valid submit, redirects on success, displays server errors. Add MSW handler for POST /api/auth/register in `src/RentalForge.Web/src/test/mocks/handlers.ts`
- [X] T019 [US1] Implement register page in `src/RentalForge.Web/src/pages/register.tsx`: form with email/password/confirmPassword fields, Zod validation, call `api.post<AuthResponse>('/api/auth/register', ...)`, display aggregated errors, link to login page
- [X] T020 [US1] Add `/register` route in `src/RentalForge.Web/src/app/routes.tsx` (outside RootLayout — public route without nav)

**Checkpoint**: Registration fully functional end-to-end. Can register a user and receive valid tokens. All validator + integration tests pass.

---

## Phase 4: User Story 2 — Login and Logout (Priority: P1)

**Goal**: Users log in with email/password, receive tokens, see their identity in the nav. Logout ends the session. Frontend auth state managed via context.

**Independent Test**: Login with seeded user via POST /api/auth/login → 200 with tokens. GET /api/auth/me with token → 200 with user info. POST /api/auth/logout → 204. GET /api/auth/me after logout token → still works (JWT stateless). Verify invalid credentials → generic 401.

### Tests (TDD Red)

- [X] T021 [P] [US2] Write `LoginRequestValidator` unit tests in `tests/RentalForge.Api.Tests/Unit/LoginRequestValidatorTests.cs`: email required/format, password required/non-empty. Use AutoFixture.
- [X] T022 [P] [US2] Write login/logout/me endpoint integration tests in `tests/RentalForge.Api.Tests/Integration/AuthEndpointTests.cs` (Login/Logout/Me sections): login success returns 200 with tokens, invalid credentials returns 401 generic message, empty fields returns 400, logout returns 204, logout with invalid token still returns 204 (idempotent), GET /me returns user info, GET /me without token returns 401

### Implementation (TDD Green)

- [X] T023 [US2] Implement `LoginRequestValidator` in `src/RentalForge.Api/Validators/LoginRequestValidator.cs`: Email required + email format, Password required + non-empty
- [X] T024 [US2] Implement `AuthService.LoginAsync`, `LogoutAsync`, `GetMeAsync` in `src/RentalForge.Api/Services/AuthService.cs`: Login — validate, find user by email, check password via `UserManager`, generic error on failure, generate JWT + RefreshToken, return AuthResponse. Logout — find refresh token, revoke entire family. GetMe — find user by ID, map to UserDto with role from `UserManager.GetRolesAsync`. Log: successful login (userId), failed login (no details beyond "invalid credentials"), logout (userId), family revocation events
- [X] T025 [US2] Implement `AuthController.Login`, `Logout`, `Me` in `src/RentalForge.Api/Controllers/AuthController.cs`: Login `[HttpPost("login")] [AllowAnonymous]`, Logout `[HttpPost("logout")] [Authorize]`, Me `[HttpGet("me")] [Authorize]` — all use result.Status switch pattern. Add `[ProducesResponseType]` attributes and XML doc summaries per contracts/auth-api.md
- [X] T026 [US2] Add Zod login schema in `src/RentalForge.Web/src/lib/validators.ts`: `loginSchema` with email + password validation
- [X] T027 [US2] Create `useAuth` hook with `AuthProvider` context in `src/RentalForge.Web/src/hooks/use-auth.tsx`: state for user/token/refreshToken, login/logout/refresh methods, initialize from localStorage on mount, expose isAuthenticated/user/role/login/logout
- [X] T028 [US2] Update `api-client.ts` in `src/RentalForge.Web/src/lib/api-client.ts`: add `getToken`/`setToken` accessor functions, attach `Authorization: Bearer <token>` header to all requests when token exists, export token accessors for useAuth integration
- [X] T029 [US2] Write login page component tests in `src/RentalForge.Web/src/pages/login.test.tsx`: renders form, shows validation errors, calls API on submit, stores tokens on success, redirects to home, shows server error on 401. Add MSW handlers for POST /api/auth/login and POST /api/auth/logout
- [X] T030 [US2] Implement login page in `src/RentalForge.Web/src/pages/login.tsx`: form with email/password, Zod validation, call login from useAuth, display generic error on failure, link to register page
- [X] T031 [US2] Add `/login` route in `src/RentalForge.Web/src/app/routes.tsx` (public, outside auth guard), update `src/RentalForge.Web/src/app/providers.tsx` to wrap with `AuthProvider` between QueryClientProvider and RouterProvider
- [X] T032 [US2] Write useAuth hook tests in `src/RentalForge.Web/src/hooks/use-auth.test.tsx`: test login stores tokens, logout clears tokens, isAuthenticated reflects state, initializes from localStorage

**Checkpoint**: Login/logout fully functional. Users can authenticate, see their identity, and end sessions. Frontend auth context manages state across the app. All tests pass.

---

## Phase 5: User Story 3 — Role-Based Access to Management Features (Priority: P2)

**Goal**: Existing endpoints enforce authorization. Customers see limited data (own rentals, own customer record, browse films). Staff sees all. Admin has full access. Unauthenticated users redirected to login. Forbidden actions show access-denied.

**Independent Test**: Login as Customer → can GET /api/films, can GET /api/customers/{ownId}, cannot GET /api/customers, cannot POST /api/films. Login as Staff → can do all CRUD. Login as Admin → same as Staff. No token → all protected endpoints return 401.

### Tests (TDD Red)

- [X] T033 [US3] Write authorization integration tests in `tests/RentalForge.Api.Tests/Integration/AuthorizationTests.cs`: test each endpoint per authorization matrix — anonymous gets 401, Customer can GET /api/films but not POST, Customer can GET own /api/customers/{id} but not others, Customer can GET own /api/rentals but not others, Customer with null CustomerId (unlinked) cannot GET /api/rentals or /api/customers/{id} but can GET /api/films and /api/auth/me, Customer with soft-deleted linked Customer record cannot access rentals (treated as unlinked), Staff can access all CRUD, Admin can access all CRUD, Health endpoint allows anonymous

### Implementation (TDD Green)

- [X] T034 [US3] Add `[Authorize]` and role-based policies to existing controllers: `HealthController` — `[AllowAnonymous]`; `CustomersController` — class-level `[Authorize(Roles = "Staff,Admin")]` + override `GetCustomer` to also allow Customer for own record; `FilmsController` — `[Authorize]` on class, `[Authorize(Roles = "Staff,Admin")]` on POST/PUT/DELETE; `RentalsController` — `[Authorize]` on class, `[Authorize(Roles = "Staff,Admin")]` on POST/PUT/DELETE, GET list/detail allow Customer with ownership filtering
- [X] T035 [US3] Implement Customer ownership scoping: update `CustomersController.GetCustomer(id)` to check if current user is Customer role and requested ID matches their `ApplicationUser.CustomerId`; update `RentalsController` GET endpoints to filter by `CustomerId` when user is Customer role; ownership check MUST also verify the linked Customer record is active (`Activebool = true`) — if soft-deleted, treat as unlinked (restrict to profile-only access). Extract current user info from `ClaimsPrincipal` (sub claim + role claim)
- [X] T036 [P] [US3] Create access-denied component in `src/RentalForge.Web/src/components/shared/access-denied.tsx`: display clear "You don't have permission to access this page" message with a link back to home
- [X] T037 [US3] Write ProtectedRoute component tests in `src/RentalForge.Web/src/components/auth/protected-route.test.tsx`: redirects to /login when unauthenticated, renders children when authenticated, shows access-denied component when role insufficient
- [X] T038 [US3] Create `ProtectedRoute` component in `src/RentalForge.Web/src/components/auth/protected-route.tsx`: check `useAuth().isAuthenticated`, redirect to /login if not, optionally check `allowedRoles` prop and render access-denied component if role not in list
- [X] T039 [US3] Update `src/RentalForge.Web/src/app/routes.tsx`: wrap all existing routes (except /login, /register) with `ProtectedRoute`, apply `allowedRoles` where needed (customers/films write routes → Staff+Admin, rentals write → Staff+Admin)
- [X] T040 [US3] Update `src/RentalForge.Web/src/components/layout/bottom-nav.tsx` and `src/RentalForge.Web/src/components/layout/sidebar-nav.tsx`: conditionally show/hide nav items based on `useAuth().role` — Customer sees Films + Rentals + Profile; Staff sees all; Admin sees all; show logout button when authenticated

**Checkpoint**: All endpoints enforce authorization. Frontend routes are protected. Navigation adapts to role. All authorization tests pass.

---

## Phase 6: User Story 4 — Session Persistence and Token Refresh (Priority: P2)

**Goal**: Sessions persist across page reloads. Access tokens refresh silently before expiry. Single-use rotation with family invalidation detects credential theft.

**Independent Test**: POST /api/auth/refresh with valid refresh token → 200 with new tokens. Using old token again → 401 + entire family revoked. Expired refresh token → 401. Frontend: reload page → still logged in.

### Tests (TDD Red)

- [X] T041 [P] [US4] Write `RefreshRequestValidator` unit tests in `tests/RentalForge.Api.Tests/Unit/RefreshRequestValidatorTests.cs`: refreshToken required/non-empty. Use AutoFixture.
- [X] T042 [US4] Write refresh endpoint integration tests in `tests/RentalForge.Api.Tests/Integration/AuthEndpointTests.cs` (Refresh section): valid refresh returns new tokens, old refresh token is consumed (reuse returns 401), reuse triggers family invalidation (all tokens for that user revoked), expired refresh returns 401, revoked refresh returns 401, new refresh token works for subsequent refresh

### Implementation (TDD Green)

- [X] T043 [US4] Implement `RefreshRequestValidator` in `src/RentalForge.Api/Validators/RefreshRequestValidator.cs`: RefreshToken required + non-empty
- [X] T044 [US4] Implement `AuthService.RefreshAsync` in `src/RentalForge.Api/Services/AuthService.cs`: validate request, look up token in DB, check active (not used, not revoked, not expired), if consumed/revoked → revoke entire family by Family ID + return Unauthorized, mark old token as consumed using optimistic concurrency (`UPDATE WHERE IsUsed = false`, check rows affected — if 0 rows updated, another request already consumed it, treat as reuse and invalidate family), create new RefreshToken with same Family, generate new JWT, return RefreshResponse. Log: successful refresh (userId, family), expired token rejection, reuse detection + family invalidation (WARNING level — potential credential theft)
- [X] T045 [US4] Implement `AuthController.Refresh` in `src/RentalForge.Api/Controllers/AuthController.cs`: `[HttpPost("refresh")] [AllowAnonymous]`, delegate to `IAuthService.RefreshAsync`, result.Status switch. Add `[ProducesResponseType<RefreshResponse>(200)]`, `[ProducesResponseType(400)]`, `[ProducesResponseType(401)]`, `[ProducesResponseType(429)]` and XML doc summary
- [X] T046 [US4] Update api-client in `src/RentalForge.Web/src/lib/api-client.ts`: before each request, check if access token is expiring within 60 seconds (decode exp claim from JWT payload via base64url decode — no signature verification needed, lightweight inline helper), if so call refresh endpoint first, update stored tokens, then proceed with original request. Handle 401 responses by attempting refresh once, then logout if refresh fails.
- [X] T047 [US4] Update `useAuth` hook in `src/RentalForge.Web/src/hooks/use-auth.tsx`: on mount, read tokens from localStorage, validate access token expiry, if expired but refresh token exists attempt refresh, set user state from JWT claims (decode sub/email/role without verification — server validates)
- [X] T048 [US4] Write frontend token refresh tests in `src/RentalForge.Web/src/lib/api-client.test.ts`: test auto-refresh when token near expiry, test 401 triggers refresh attempt, test failed refresh clears auth state. Add MSW handler for POST /api/auth/refresh.

**Checkpoint**: Tokens refresh transparently. Sessions persist across page reloads. Credential reuse is detected and families are invalidated. All tests pass.

---

## Phase 7: User Story 5 — View Own Profile (Priority: P3)

**Goal**: Profile page shows real user data (email, role, created date) and linked dvdrental Customer data if available.

**Independent Test**: Login → navigate to /profile → see email, role, creation date. If Customer with linked record → also see customer name, store, address.

### Tests (TDD Red)

- [X] T049 [US5] Write profile page tests in `src/RentalForge.Web/src/pages/profile.test.tsx`: renders auth user data (email, role, createdAt) from useAuth, fetches and displays linked customer data when customerId present, shows "no linked customer" message when customerId is null. Add MSW handlers for GET /api/auth/me and GET /api/customers/{id}.

### Implementation (TDD Green)

- [X] T050 [US5] Update profile page in `src/RentalForge.Web/src/pages/profile.tsx`: replace placeholder with real data from `useAuth()` for auth info (email, role, createdAt), conditionally fetch `GET /api/customers/{customerId}` via existing `useCustomer` hook when `customerId` is non-null, display customer name/store/address details, show appropriate message when not linked
- [X] T051 [US5] Add GET /api/auth/me MSW handler in `src/RentalForge.Web/src/test/mocks/handlers.ts` if not already added in earlier phases

**Checkpoint**: Profile page shows real user data. Linked customer details display when available. Tests pass.

---

## Phase 8: User Story 6 — Rate Limiting on Authentication Endpoints (Priority: P3)

**Goal**: Auth endpoints are rate-limited per spec thresholds. Users see "too many requests" message when rate-limited.

**Independent Test**: Send 6 rapid login requests → 6th gets 429. Send 4 rapid register requests → 4th gets 429. Send 11 rapid refresh requests → 11th gets 429. Wait for window reset → next request succeeds.

### Tests (TDD Red)

- [X] T052 [US6] Write rate limit integration tests in `tests/RentalForge.Api.Tests/Integration/RateLimitTests.cs`: test login rate limit (6th request in 1 min → 429), register rate limit (4th request → 429), refresh rate limit (11th request → 429), verify 429 response includes Retry-After header, verify requests succeed after window reset. Use `FakeTimeProvider` (registered via `TimeProvider` DI override in TestWebAppFactory) to advance time deterministically instead of real delays

### Implementation (TDD Green)

- [X] T053 [US6] Configure rate limiting middleware in `src/RentalForge.Api/Program.cs`: `AddRateLimiter()` with three fixed-window policies — `auth-login` (5/min), `auth-register` (3/min), `auth-refresh` (10/min); add `UseRateLimiter()` in pipeline before `UseAuthentication()`
- [X] T054 [US6] Apply rate limit policies to `AuthController` endpoints in `src/RentalForge.Api/Controllers/AuthController.cs`: `[EnableRateLimiting("auth-login")]` on Login, `[EnableRateLimiting("auth-register")]` on Register, `[EnableRateLimiting("auth-refresh")]` on Refresh
- [X] T055 [US6] Add frontend rate-limit error handling: update `ApiError` handling in `src/RentalForge.Web/src/lib/api-client.ts` to recognize 429 status, display toast notification via Sonner with "Too many requests. Please try again later." message in login and register pages

**Checkpoint**: Auth endpoints are rate-limited. 429 responses include Retry-After. Frontend displays clear rate-limit messages. All tests pass.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Seeding, final integration, and end-to-end validation

- [X] T056 Update `DevDataSeeder` in `src/RentalForge.Api/Data/Seeding/DevDataSeeder.cs`: seed three default auth users (admin@rentalforge.dev/Admin, staff@rentalforge.dev/Staff, customer@rentalforge.dev/Customer linked to existing Customer record) using `UserManager<ApplicationUser>`, skip if already exist
- [X] T057 Add EF Core migration: run `dotnet ef migrations add AddIdentitySchema` from `src/RentalForge.Api/` and verify migration creates `identity` schema with all expected tables
- [X] T058 Run full backend test suite (`dotnet test`) — verify all existing tests (206+) still pass alongside new auth tests
- [X] T059 Run full frontend test suite (`npm test` in `src/RentalForge.Web/`) — verify all existing tests still pass alongside new auth tests
- [X] T060 Validate quickstart.md end-to-end: configure user secrets, run migration, seed users, test curl commands from quickstart, verify all responses match contracts

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1 Register)**: Depends on Phase 2
- **Phase 4 (US2 Login/Logout)**: Depends on Phase 2 (independent of US1 — uses UserManager directly in tests)
- **Phase 5 (US3 Role-Based Access)**: Depends on Phase 2 + Phase 4 (needs AuthTestHelper tokens for testing)
- **Phase 6 (US4 Token Refresh)**: Depends on Phase 2 + Phase 4 (extends AuthService with refresh logic)
- **Phase 7 (US5 Profile)**: Depends on Phase 4 (needs useAuth hook) + Phase 5 (needs Customer ownership access)
- **Phase 8 (US6 Rate Limiting)**: Depends on Phase 2 (can be parallel with US3-US5)
- **Phase 9 (Polish)**: Depends on all prior phases

### User Story Dependencies

```
Phase 2 (Foundational)
  ├── US1 (Register) ─────────────────┐
  ├── US2 (Login/Logout) ─────────────┤
  │     ├── US3 (Role-Based Access) ──┤
  │     ├── US4 (Token Refresh) ──────┤
  │     └── US5 (Profile) ────────────┤ → Phase 9 (Polish)
  └── US6 (Rate Limiting) ────────────┘
```

- US1 and US2 can run in parallel (both P1, independent test paths)
- US3 and US4 can run in parallel after US2 (both P2, different concerns)
- US6 can run in parallel with US3-US5 (only touches Program.cs + controller attributes)
- US5 depends on US3 (needs Customer ownership access) and US4 (needs useAuth with persistence)

### Within Each User Story

1. Unit tests for validators → FAIL (red)
2. Validator implementation → PASS (green)
3. Integration tests for endpoints → FAIL (red)
4. Service implementation → (partial green)
5. Controller implementation → PASS (green)
6. Frontend tests → FAIL (red)
7. Frontend implementation → PASS (green)

### Parallel Opportunities

**Phase 2** (within phase):
```
T003 (ApplicationUser entity)  ──┐
T004 (RefreshToken entity)     ──┼── parallel
T006 (Auth DTOs)               ──┘
  Then: T005 (DbContext) → T007 (IAuthService) → T008 (Program.cs) → T009/T010 (test infra)
```

**US1 + US2** (across stories after Phase 2):
```
US1: T011-T020 (register flow)
US2: T021-T032 (login/logout flow)
  Both can proceed simultaneously
```

**US3 + US4 + US6** (after US2):
```
US3: T033-T040 (authorization)
US4: T041-T048 (token refresh)
US6: T052-T055 (rate limiting)
  All three can proceed simultaneously
```

---

## Parallel Example: User Story 1

```text
# Launch validator tests + frontend types in parallel:
T011: RegisterRequestValidator unit tests       (tests/Unit/)
T012: Register integration tests                (tests/Integration/)
T016: Frontend auth types                       (src/RentalForge.Web/src/types/)
T017: Zod register schema                       (src/RentalForge.Web/src/lib/)

# After tests written, implement sequentially:
T013 → T014 → T015 (backend: validator → service → controller)

# Then frontend:
T018 → T019 → T020 (tests → page → route)
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2)

1. Complete Phase 1: Setup (2 tasks)
2. Complete Phase 2: Foundational (8 tasks)
3. Complete Phase 3: US1 Register (10 tasks)
4. Complete Phase 4: US2 Login/Logout (12 tasks)
5. **STOP and VALIDATE**: Users can register, login, logout, see identity in nav
6. This is the minimum viable auth system

### Incremental Delivery

1. Setup + Foundational → Infrastructure ready
2. US1 (Register) → Users can create accounts
3. US2 (Login/Logout) → Users can authenticate (MVP!)
4. US3 (Role-Based Access) → Endpoints protected, nav adapts to role
5. US4 (Token Refresh) → Sessions persist, tokens rotate
6. US5 (Profile) → Real profile data
7. US6 (Rate Limiting) → Security hardening
8. Polish → Seeding, final validation

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks in same phase
- [US*] label maps task to specific user story for traceability
- All backend tests use `TestWebAppFactory` with Testcontainers (disposable PostgreSQL)
- All frontend tests use MSW for API mocking
- AutoFixture used in validator unit tests for anonymous data generation
- Existing 206+ backend tests must continue passing (no regressions)
- Commit after each completed task or logical group
- Stop at any checkpoint to validate story independently
