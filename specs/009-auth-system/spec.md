# Feature Specification: Authentication System

**Feature Branch**: `009-auth-system`
**Created**: 2026-02-24
**Status**: Draft
**Input**: User description: "Implement full Authentication system covering both backend and frontend. ASP.NET Core Identity + JWT bearer tokens. Roles: Admin, Staff, Customer. Rate limiting on auth endpoints. React auth context + JWT storage + refresh tokens. Protected routes. Role-based UI rendering. Full TDD."

## Clarifications

### Session 2026-02-24

- Q: When a refresh credential is used to obtain a new access credential, should the old refresh credential be invalidated (single-use rotation) or remain reusable until expiry? → A: Single-use rotation with family invalidation. Each refresh issues a new refresh credential and invalidates the old one. Reuse of an already-consumed credential invalidates the entire credential family (all sessions for that user), forcing re-authentication.
- Q: How does a Customer-role auth User link to the existing dvdrental Customer entity for "own data" scoping? → A: Explicit foreign key. The auth User entity has a nullable FK to the dvdrental Customer record. Populated at registration or by an Admin. Customer-role users only see rentals for their linked Customer record.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Account Registration (Priority: P1)

A new user visits the application and creates an account by providing their email address and a password. The system validates the information, creates the account, and immediately logs the user in so they can begin using the application. Registration defaults to the Customer role. Only administrators can register users with elevated roles (Staff or Admin).

**Why this priority**: Without registration, no user can enter the system. This is the foundational entry point that all other auth stories depend on.

**Independent Test**: Can be fully tested by navigating to the registration page, filling out the form, submitting, and verifying the user is logged in and can access customer-level features.

**Acceptance Scenarios**:

1. **Given** a visitor on the registration page, **When** they provide a valid email and password meeting strength requirements, **Then** the account is created, the user is logged in, and redirected to the home page with their name displayed.
2. **Given** a visitor on the registration page, **When** they provide an email that is already registered, **Then** the system displays an error message indicating the email is taken, without revealing whether the account exists (generic message).
3. **Given** a visitor on the registration page, **When** they provide a password that does not meet strength requirements, **Then** the system displays all validation errors at once (not one at a time).
4. **Given** an authenticated Admin user, **When** they register a new user with an elevated role (Staff or Admin), **Then** the account is created with that role.
5. **Given** an authenticated non-Admin user or unauthenticated visitor, **When** they attempt to register with a role other than Customer, **Then** the system rejects the request.

---

### User Story 2 - Login and Logout (Priority: P1)

A registered user logs in with their email and password to access the application. The system authenticates the credentials and provides a session. The user can also log out, which ends their session and returns them to a public view.

**Why this priority**: Login/logout is equally foundational to registration — users must be able to start and end authenticated sessions.

**Independent Test**: Can be fully tested by logging in with valid credentials, verifying access to protected content, then logging out and verifying protected content is no longer accessible.

**Acceptance Scenarios**:

1. **Given** a registered user on the login page, **When** they enter valid credentials, **Then** they are authenticated, redirected to the home page, and their name and role are displayed in the navigation.
2. **Given** a user on the login page, **When** they enter an incorrect email or password, **Then** the system displays a generic "invalid credentials" error (not specifying which field was wrong).
3. **Given** an authenticated user, **When** they click logout, **Then** their session is ended, they are redirected to the login page, and they can no longer access protected content.
4. **Given** a user who has failed login multiple times in rapid succession, **When** they attempt another login, **Then** the system temporarily blocks further attempts and displays a rate-limit message.

---

### User Story 3 - Role-Based Access to Management Features (Priority: P2)

Different users see different parts of the application based on their role. Customers can view the catalog and manage their own rentals. Staff can manage all customers, films, and rentals. Admins have full access including user management. Attempting to access a feature beyond one's role results in a clear denial.

**Why this priority**: Authorization boundaries define what each user type can do — critical for data integrity and business rules, but depends on auth being functional first.

**Independent Test**: Can be tested by logging in as each role type (Customer, Staff, Admin) and verifying which pages and actions are accessible vs. denied.

**Acceptance Scenarios**:

1. **Given** an authenticated Customer whose account is linked to a dvdrental Customer record, **When** they navigate the application, **Then** they can browse films, view rentals belonging to their linked Customer record, and access their profile — but cannot access customer management, film management, or rental management for other users.
1a. **Given** an authenticated Customer whose account is NOT linked to a dvdrental Customer record, **When** they navigate the application, **Then** they can access their profile and browse films, but cannot view any rentals until the link is established.
2. **Given** an authenticated Staff member, **When** they navigate the application, **Then** they can access customer management, film management, and rental management (all CRUD operations).
3. **Given** an authenticated Admin, **When** they navigate the application, **Then** they have all Staff capabilities plus the ability to register users with elevated roles.
4. **Given** an authenticated user of any role, **When** they attempt to access a page or action beyond their role, **Then** they see a clear "access denied" message (not a broken page or cryptic error).
5. **Given** an unauthenticated visitor, **When** they attempt to access any protected page, **Then** they are redirected to the login page.

---

### User Story 4 - Session Persistence and Token Refresh (Priority: P2)

A user's session persists across page reloads and browser restarts without requiring re-login, up to a defined session lifetime. When the short-lived access credential is about to expire, the system silently refreshes it in the background so the user experiences no interruption.

**Why this priority**: Session continuity is essential for usability — forcing frequent re-logins would make the application frustrating — but it only matters once login itself works.

**Independent Test**: Can be tested by logging in, waiting for the access credential to near expiry, performing an action, and verifying it succeeds without a login prompt. Also test that refreshing the browser page preserves the logged-in state.

**Acceptance Scenarios**:

1. **Given** an authenticated user whose access credential is about to expire, **When** they perform any action, **Then** the system silently renews the credential and the action succeeds without interruption.
2. **Given** an authenticated user, **When** they reload the browser page, **Then** they remain logged in with the same role and permissions.
3. **Given** a user whose refresh credential has expired (e.g., after extended inactivity), **When** they attempt any action, **Then** they are redirected to the login page with a message indicating their session has expired.
4. **Given** a user who has logged out, **When** they attempt to use a previously valid refresh credential, **Then** the system rejects it (the credential is invalidated on logout).

---

### User Story 5 - View Own Profile (Priority: P3)

An authenticated user can view their profile information (email, role, account status). This replaces the current placeholder profile page with real user data.

**Why this priority**: Profile viewing is a supporting feature that enhances the user experience but is not critical to core auth flows.

**Independent Test**: Can be tested by logging in and navigating to the profile page, verifying that the displayed information matches the logged-in user's account.

**Acceptance Scenarios**:

1. **Given** an authenticated user, **When** they navigate to their profile, **Then** they see their email address, assigned role, and account creation date.
2. **Given** an unauthenticated visitor, **When** they attempt to access the profile page, **Then** they are redirected to the login page.

---

### User Story 6 - Rate Limiting on Authentication Endpoints (Priority: P3)

The system limits the rate of authentication-related requests (registration, login, token refresh) to prevent abuse. Users who exceed the rate limit receive a clear message and must wait before retrying.

**Why this priority**: Rate limiting is a security hardening measure. The system functions without it, but it protects against brute-force and credential-stuffing attacks.

**Independent Test**: Can be tested by sending rapid repeated requests to authentication endpoints and verifying that the system begins rejecting them after the threshold is reached.

**Acceptance Scenarios**:

1. **Given** any client, **When** they send more than 5 login attempts within 1 minute, **Then** subsequent attempts are rejected with a "too many requests" response until the window resets.
2. **Given** any client, **When** they send more than 3 registration requests within 1 minute, **Then** subsequent attempts are rejected with a "too many requests" response.
3. **Given** any client, **When** they send more than 10 token refresh requests within 1 minute, **Then** subsequent attempts are rejected with a "too many requests" response.
4. **Given** a rate-limited client, **When** the rate-limit window resets, **Then** they can resume normal authentication operations.

---

### Edge Cases

- What happens when a user's role is changed while they have an active session? The current session retains the old role until the access credential expires and is refreshed, at which point the new role takes effect.
- What happens when two sessions attempt to refresh at the same instant? Each refresh request should be processed independently without conflict.
- What happens when the system has seeded default users and someone tries to register with the same email? Registration is rejected with the standard "email taken" error.
- What happens when a user submits a registration or login form with an empty email or empty password? All validation errors are displayed simultaneously (never early-return on first failure).
- What happens when a malformed or tampered credential is presented? The system rejects the request and treats it as unauthenticated.
- What happens when a previously consumed refresh credential is reused (potential theft)? The system invalidates all refresh credentials in that family, forcing the legitimate user and any attacker to re-authenticate. The reuse attempt is rejected.
- What happens when a Customer-role user registers but has no linked dvdrental Customer record? They can access their profile and browse films but cannot view rentals. An Admin or Staff member can later link them to a Customer record.
- What happens when a linked dvdrental Customer record is soft-deleted? The auth User account remains active but rental access is restricted as if unlinked, until a new Customer record is linked.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow visitors to register an account with an email address and password.
- **FR-002**: System MUST validate registration input and display all validation errors simultaneously (never one at a time).
- **FR-003**: System MUST enforce password strength requirements (minimum 8 characters, at least one uppercase letter, one lowercase letter, one digit, and one non-alphanumeric character).
- **FR-004**: System MUST default new registrations to the Customer role; only Admin users may assign Staff or Admin roles during registration.
- **FR-005**: System MUST allow registered users to log in with email and password.
- **FR-006**: System MUST return a generic error for failed login attempts that does not reveal whether the email or password was incorrect.
- **FR-007**: System MUST return the authenticated user's identity, role, and credentials upon successful login.
- **FR-008**: System MUST allow authenticated users to log out, invalidating their current session credentials.
- **FR-009**: System MUST provide short-lived access credentials and longer-lived refresh credentials to maintain sessions.
- **FR-010**: System MUST silently refresh access credentials before they expire, without user intervention.
- **FR-011**: System MUST invalidate refresh credentials upon logout so they cannot be reused.
- **FR-011a**: System MUST implement single-use refresh credential rotation: each successful refresh issues a new refresh credential and invalidates the previous one.
- **FR-011b**: System MUST detect reuse of a previously consumed refresh credential and, upon detection, invalidate all refresh credentials in that family (all sessions for that user), forcing re-authentication.
- **FR-012**: System MUST allow authenticated users to view their own profile (email, role, account creation date).
- **FR-013**: System MUST enforce role-based access control on all management endpoints: Customer (own data only, scoped via explicit User-to-Customer link), Staff (all operational data), Admin (full access including user role assignment).
- **FR-013a**: System MUST support a nullable link from an auth User to a dvdrental Customer record. This link is populated at registration or by an Admin.
- **FR-013b**: Customer-role users whose account is not yet linked to a dvdrental Customer record MUST be restricted to profile-only access until the link is established.
- **FR-014**: System MUST redirect unauthenticated users to the login page when they attempt to access protected content.
- **FR-015**: System MUST display an "access denied" message when users attempt actions beyond their role.
- **FR-016**: System MUST rate-limit authentication endpoints: login (5 per minute), registration (3 per minute), token refresh (10 per minute).
- **FR-017**: System MUST display a clear "too many requests" message to rate-limited users.
- **FR-018**: System MUST use the platform's built-in password hashing (no custom password hashing).
- **FR-019**: System MUST persist authentication state across page reloads and browser restarts (within the session lifetime).
- **FR-020**: System MUST adapt the navigation and visible pages/actions based on the authenticated user's role.
- **FR-021**: System MUST seed default users for each role (one Admin, one Staff, one Customer) for development and testing purposes.
- **FR-022**: System MUST aggregate all validation errors before responding — never early-return on first failure.

### Key Entities

- **User**: Represents an authenticated person in the system. Key attributes: email (unique identifier), hashed password, assigned role, account creation timestamp, active status, optional link to a dvdrental Customer record (nullable foreign key). The Customer link enables per-record data scoping for Customer-role users.
- **Role**: Defines a permission level within the system. Three fixed roles: Admin (full access), Staff (day-to-day operations), Customer (own data only). Roles are seeded, not user-created.
- **Access Credential**: A short-lived token proving the user's identity and role. Issued on login and refresh. Contains the user's identity and role claims. Expires after a short period (e.g., 15 minutes).
- **Refresh Credential**: A longer-lived, single-use token used to obtain new access credentials without re-entering login details. Issued on login and on each refresh (rotation). Invalidated on logout or after use. Expires after a longer period (e.g., 7 days). Belongs to a credential family; reuse of a consumed credential invalidates the entire family.
- **Credential Family**: A chain of refresh credentials originating from a single login event. Used to detect credential theft — if a consumed credential is reused, all credentials in the family are revoked.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete registration and reach the home page in under 30 seconds.
- **SC-002**: Users can log in and reach the home page in under 10 seconds.
- **SC-003**: Token refresh occurs transparently — users experience zero interruptions during a continuous session lasting up to 7 days.
- **SC-004**: 100% of protected pages redirect unauthenticated users to login within 1 second.
- **SC-005**: Role-based navigation renders correctly for all three roles — each role sees only the features they are authorized to use.
- **SC-006**: Rate limiting activates within 1 second of threshold breach and blocks further attempts until the window resets.
- **SC-007**: All validation errors are displayed simultaneously — users never see a single error followed by a different error after correcting the first.
- **SC-008**: Login error messages never reveal whether the email or password was the incorrect field.
- **SC-009**: Logged-out users cannot reuse previously valid credentials to access protected content.
- **SC-010**: All authentication features are covered by automated tests following test-driven development practices.

## Assumptions

- The existing `dvdrental` database schema is separate from the identity/auth data store. Auth entities (users, roles, tokens) will coexist in the same database but in their own tables, not conflicting with the existing `dvdrental` schema.
- The three seeded default users are for development and testing only — production deployment would use a separate seeding strategy. The seeded Customer-role user will be linked to an existing dvdrental Customer record for testing purposes.
- Password strength requirements follow industry-standard defaults (min 8 chars, mixed case, digit, special character) unless the user specifies otherwise.
- Access credential lifetime of ~15 minutes and refresh credential lifetime of ~7 days are reasonable defaults. These are documented here as assumptions and can be adjusted during planning.
- The existing Profile page placeholder (from feature #008) will be replaced with real profile content showing the authenticated user's information.
- The health endpoint (`GET /health`) remains publicly accessible and does not require authentication.
- Film and customer browsing (read-only list/detail) will remain accessible to all authenticated users regardless of role. Write operations (create, update, delete) require Staff or Admin.

## Scope Boundaries

### In Scope

- User registration (email + password, role assignment)
- User login and logout
- Access and refresh credential issuance and renewal
- Role-based access control (Admin, Staff, Customer)
- Rate limiting on authentication endpoints
- Frontend auth state management, protected routes, and role-based UI
- Seeding default users (one per role)
- Updating existing controllers with appropriate access control
- Updating existing frontend to integrate auth state
- Full TDD coverage on both backend and frontend

### Out of Scope

- Password reset / "forgot password" flow
- Email confirmation / verification
- Social login (OAuth, Google, GitHub, etc.)
- Two-factor authentication (2FA)
- Account lockout after failed attempts (rate limiting covers abuse prevention)
- User self-service role changes
- Admin dashboard for user management CRUD (Admin can only assign roles during registration for now)
