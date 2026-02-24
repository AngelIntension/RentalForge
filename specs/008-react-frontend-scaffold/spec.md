# Feature Specification: React Frontend Scaffold

**Feature Branch**: `008-react-frontend-scaffold`
**Created**: 2026-02-23
**Status**: Draft
**Input**: User description: "Scaffold a complete React 19 + TypeScript (strict mode) SPA frontend in the monorepo under src/RentalForge.Web with Vite, React Router v7, TanStack Query v5, Zod, Tailwind CSS + shadcn/ui, PWA support, and placeholder pages for existing APIs."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse and Search Film Catalog (Priority: P1)

A customer opens the application on their phone or desktop and sees the film catalog. They can scroll through films, search by title or actor name, and filter by category or rating. Tapping a film shows its details including description, actors, categories, rental rate, and length.

**Why this priority**: Browsing the film catalog is the core value proposition of a rental store. Without it, no other feature is useful. This story exercises the most complex existing API (search, filtering, pagination) and proves the frontend can communicate with the backend.

**Independent Test**: Can be fully tested by loading the films page, searching for a film by name, applying a category filter, and viewing film details. Delivers immediate value as a browsable catalog.

**Acceptance Scenarios**:

1. **Given** the application is loaded, **When** the user navigates to Browse Films, **Then** the system displays a paginated list of films with title, rating, year, and rental rate
2. **Given** the film list is displayed, **When** the user types a search term, **Then** the list filters to show matching films by title, description, or actor name
3. **Given** the film list is displayed, **When** the user selects a category or rating filter, **Then** the list shows only films matching the selected filters
4. **Given** a film is displayed in the list, **When** the user taps on the film, **Then** the system shows the film detail view with description, actors, categories, language, rental rate, length, and special features
5. **Given** more films match than fit on one page, **When** the user scrolls down or taps "Load More", **Then** additional films append to the list seamlessly without replacing existing results

---

### User Story 2 - View and Search Customers (Priority: P2)

A staff member opens the application and navigates to a customer list. They can search for customers by name and view customer details. This supports daily rental operations where staff need to look up customers.

**Why this priority**: Customer lookup is essential for rental operations (you need a customer to create a rental). This story validates a second API integration with a simpler data model.

**Independent Test**: Can be tested by loading the customers page, searching for a customer by name, and viewing customer details. Delivers value for staff lookup workflows.

**Acceptance Scenarios**:

1. **Given** the application is loaded, **When** the user navigates to the customers area, **Then** the system displays a paginated list of active customers with name and email
2. **Given** the customer list is displayed, **When** the user types a search term, **Then** the list filters to show customers matching by name
3. **Given** a customer is displayed in the list, **When** the user selects the customer, **Then** the system shows the customer detail view with all customer information

---

### User Story 3 - View Rentals and Process Rental Transactions (Priority: P3)

A staff member views the rental list, optionally filtered by customer or active status. They can create a new rental for a customer by selecting a film and store, and they can process a return on an active rental. The system automatically resolves available inventory.

**Why this priority**: Rental management is the primary transactional workflow but depends on films and customers being browsable first. This story validates write operations (POST, PUT) in addition to reads.

**Independent Test**: Can be tested by viewing the rental list, creating a new rental, and processing a return. Delivers the complete rental transaction workflow.

**Acceptance Scenarios**:

1. **Given** the application is loaded, **When** the user navigates to Rentals, **Then** the system displays a paginated list of rentals with rental date, return status (active/returned), customer ID, and inventory ID
2. **Given** the rental list is displayed, **When** the user filters by active rentals only, **Then** the list shows only rentals without a return date
3. **Given** the user wants to rent a film, **When** they select a customer, film, and store and submit the rental, **Then** the system creates the rental and shows the rental confirmation with details
4. **Given** an active rental is displayed, **When** the user initiates a return, **Then** the system processes the return and updates the rental to show the return date
5. **Given** a rental creation fails due to no available inventory, **When** the system responds with an error, **Then** the user sees a clear error message explaining no copies are available
6. **Given** a rental is displayed in the list, **When** the user taps on it, **Then** the system shows the rental detail view with film title, customer name, staff name, rental date, and return status

---

### User Story 4 - Install Application on Mobile Device (Priority: P4)

A user visits the application URL on their mobile phone browser. The browser prompts them to install the application to their home screen. Once installed, the app launches like a native application with its own icon, splash screen, and full-screen experience.

**Why this priority**: PWA installability dramatically improves the mobile experience and user retention, but it's an enhancement over the core browsing functionality.

**Independent Test**: Can be tested by visiting the app URL on a mobile browser, accepting the install prompt, and launching from the home screen. Delivers native-like mobile experience.

**Acceptance Scenarios**:

1. **Given** a user visits the application URL on a mobile browser, **When** the PWA criteria are met, **Then** the browser offers to install the application
2. **Given** the user has installed the application, **When** they launch it from their home screen, **Then** the app opens in full-screen mode with the application icon and name
3. **Given** the application is installed, **When** the user navigates between pages within a session, **Then** previously fetched data renders instantly from cache while fresh data loads in the background

---

### User Story 5 - Switch Between Light and Dark Mode (Priority: P5)

A user prefers dark mode for nighttime browsing or personal preference. They can toggle between light and dark mode, and the application remembers their preference across sessions. The application also respects the device's system theme preference by default.

**Why this priority**: Dark mode is a quality-of-life enhancement that improves accessibility and user comfort but does not affect core functionality.

**Independent Test**: Can be tested by toggling the theme switch and verifying all pages render correctly in both modes. Delivers improved visual comfort.

**Acceptance Scenarios**:

1. **Given** the application is loaded for the first time, **When** the user's device is set to dark mode, **Then** the application renders in dark mode by default
2. **Given** the application is in light mode, **When** the user toggles the theme, **Then** all pages and components switch to dark mode
3. **Given** the user has selected a theme preference, **When** they close and reopen the application, **Then** their preference is preserved

---

### Edge Cases

- What happens when the backend API is unreachable? The application MUST display a user-friendly error state indicating connectivity issues, not a blank page or raw error.
- What happens when an API request returns an empty result set? The application MUST display an appropriate empty state message (e.g., "No films found matching your search").
- What happens when a user accesses a direct URL (deep link) to a specific page? The application MUST route correctly to that page without requiring navigation from the home page.
- What happens when the user is on a slow network? Loading states MUST be visible so the user knows data is being fetched.
- What happens when API pagination parameters are invalid? The application MUST handle error responses gracefully and display a meaningful message.
- What happens when the viewport is very narrow (< 320px) or very wide (> 2560px)? The layout MUST remain usable at extreme viewport sizes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST present a mobile-first responsive layout that adapts from phone screens (320px) to desktop monitors (2560px+)
- **FR-002**: The application MUST provide persistent bottom navigation on mobile viewports with tabs for Home, Browse Films, Rentals, and Profile
- **FR-003**: The application MUST adapt navigation to a sidebar or top bar on wider desktop viewports
- **FR-004**: The application MUST display a paginated, searchable film catalog that queries the existing Films API with support for search, category filter, rating filter, and year range filter
- **FR-005**: The application MUST display a film detail view showing all film attributes including actors, categories, language, and special features
- **FR-006**: The application MUST display a paginated, searchable customer list that queries the existing Customers API
- **FR-007**: The application MUST display a customer detail view showing all customer attributes
- **FR-008**: The application MUST display a paginated rental list with filtering by customer and active status, querying the existing Rentals API
- **FR-009**: The application MUST provide a form to create a new rental by selecting a customer, film, store, and staff member (staff selection is a temporary placeholder until authentication auto-populates this in #009)
- **FR-010**: The application MUST provide the ability to process a rental return on active rentals
- **FR-011**: All API communication MUST go through a single centralized API client — no scattered direct calls to the backend
- **FR-012**: The application MUST display clear loading indicators while data is being fetched
- **FR-013**: The application MUST display user-friendly error messages when API calls fail, including network errors and validation errors
- **FR-014**: The application MUST display appropriate empty state messages when no results match a query
- **FR-015**: The application MUST support deep linking — navigating directly to any page via URL
- **FR-016**: The application MUST support light mode and dark mode with a user-accessible toggle
- **FR-017**: The application MUST default to the user's system theme preference on first visit
- **FR-018**: The application MUST persist the user's theme preference across sessions
- **FR-019**: The application MUST be installable as a Progressive Web App on supported mobile browsers
- **FR-020**: The installed PWA MUST display a custom icon, name, and launch in standalone (full-screen) mode
- **FR-021**: The application MUST coexist in the monorepo without interfering with backend build, test, or run commands
- **FR-022**: The application MUST validate user input on forms before submitting to the API (e.g., required fields on rental creation)
- **FR-023**: The Home page MUST display a landing/dashboard view with quick access to the main features, including a link to the customer search/list view
- **FR-024**: The customer list and detail views MUST be accessible via URL routing (deep-linkable) even though they are not top-level navigation tabs
- **FR-025**: All list views (films, customers, rentals) MUST use infinite scroll pagination with a "Load More" button fallback, appending new results to the existing list without replacing previously loaded items

### Key Entities

- **Film**: A movie available for rental — attributes include title, description, release year, rating, rental rate, rental duration, length, replacement cost, special features, language, actors, and categories
- **Customer**: A person who rents films — attributes include first name, last name, email, active status, store assignment, and address assignment
- **Rental**: A transaction where a customer rents a film copy — attributes include rental date, return date (null if active), and links to inventory, customer, and staff

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can find a specific film by searching in under 5 seconds on a standard mobile connection
- **SC-002**: All pages load and display content within 3 seconds on a 3G mobile connection (first meaningful paint)
- **SC-003**: Users can complete a rental transaction (select customer, select film, submit) in under 30 seconds
- **SC-004**: Users can process a rental return in under 10 seconds (2 taps from the rental list)
- **SC-005**: The application renders correctly on viewports from 320px to 2560px wide without horizontal scrolling or content overflow
- **SC-006**: The application passes PWA installability criteria on Chrome and Safari mobile browsers
- **SC-007**: Theme switching (light/dark) applies to all visible components with zero un-themed elements
- **SC-008**: All critical user interactions (search, filter, navigate, create rental, process return) have automated test coverage
- **SC-009**: The frontend build and test commands complete successfully without affecting backend build or test commands
- **SC-010**: 100% of API calls are routed through the centralized API client with zero direct fetch calls in page components

## Clarifications

### Session 2026-02-23

- Q: Where does customer management fit in the 4-tab mobile navigation? → A: Customers are accessible via the Home page dashboard link, not a top-level navigation tab. The 4-tab mobile nav (Home, Browse Films, Rentals, Profile) remains unchanged.
- Q: What pagination UX pattern should list views use? → A: Infinite scroll with a "Load More" button fallback across all list views (films, customers, rentals).

## Assumptions

- The existing backend API (Customers, Films, Rentals, Health endpoints) is running and accessible at a configurable base URL
- Authentication is not yet implemented (deferred to feature #009) — all API calls are unauthenticated for this feature
- The Profile tab in bottom navigation will be a placeholder page for now, to be populated when auth is added in #009
- The Home page is a simple landing/dashboard page with links to the main features, not a complex analytics dashboard
- Rental creation form will use simple select/search inputs for customer and film selection — no advanced autocomplete or multi-step wizards
- The backend API already handles all business validation — the frontend performs only basic input presence/format validation before submission
- No offline-first data caching strategy is needed for this feature — PWA support is limited to installability, custom icon, and basic caching of static assets
