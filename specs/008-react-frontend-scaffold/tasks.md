# Tasks: React Frontend Scaffold

**Input**: Design documents from `/specs/008-react-frontend-scaffold/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api-client.md, quickstart.md

**Tests**: TDD is required per constitution. Test tasks are included for all hooks and the API client (RED before GREEN). Presentation components include tests as part of implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Project Initialization)

**Purpose**: Scaffold the Vite project, install all dependencies, and configure tooling

- [ ] T001 Scaffold Vite + React 19 + TypeScript project in src/RentalForge.Web/ using `npm create vite@latest RentalForge.Web -- --template react-ts` from the src/ directory; remove generated demo files (App.tsx, App.css, assets/) but keep index.css, main.tsx, and vite-env.d.ts
- [ ] T002 Install all additional dependencies in src/RentalForge.Web/: runtime (react-router, @tanstack/react-query, @tanstack/react-query-devtools, zod, tailwindcss, @tailwindcss/vite, vite-plugin-pwa) and dev/test (@testing-library/react, @testing-library/jest-dom, @testing-library/user-event, vitest, jsdom@^26.1.0, msw, @types/node)
- [ ] T003 [P] Configure Vite in src/RentalForge.Web/vite.config.ts with @vitejs/plugin-react, @tailwindcss/vite plugin, and `@` path alias resolving to `./src`; update src/RentalForge.Web/src/index.css to contain only `@import "tailwindcss";`
- [ ] T004 [P] Configure TypeScript path aliases (`@/*` → `./src/*`) in src/RentalForge.Web/tsconfig.app.json and tsconfig.json; create src/RentalForge.Web/.env.example with `VITE_API_BASE_URL=http://localhost:5000`; update src/RentalForge.Web/package.json with npm scripts: test (`vitest run`), test:watch (`vitest`), test:coverage (`vitest run --coverage`), typecheck (`tsc -b --noEmit`), lint (`eslint .`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 Initialize shadcn/ui in src/RentalForge.Web/ using `npx shadcn@latest init` (New York style, neutral base color); then add base components: `npx shadcn@latest add button card input select skeleton badge label separator sonner dropdown-menu`
- [ ] T006 [P] Create all TypeScript type definitions per data-model.md: src/RentalForge.Web/src/types/api.ts (PagedResponse\<T\>, ApiError), src/RentalForge.Web/src/types/film.ts (FilmListItem, FilmDetail, MpaaRating, FilmSearchParams), src/RentalForge.Web/src/types/customer.ts (CustomerListItem, CustomerSearchParams), src/RentalForge.Web/src/types/rental.ts (RentalListItem, RentalDetail, CreateRentalRequest, RentalSearchParams); create Zod schemas in src/RentalForge.Web/src/lib/validators.ts (createRentalSchema with z.coerce.number().int().positive() for filmId, storeId, customerId, staffId; derive CreateRentalFormData type via z.infer)
- [ ] T007 [P] Configure Vitest in src/RentalForge.Web/vitest.config.ts (environment: jsdom, globals: true, setupFiles pointing to src/test/setup.ts, path alias); create test setup file src/RentalForge.Web/src/test/setup.ts importing @testing-library/jest-dom/vitest and configuring MSW server lifecycle (beforeAll/afterEach/afterAll)
- [ ] T008 [P] Create MSW mock server in src/RentalForge.Web/src/test/mocks/server.ts; create request handlers in src/RentalForge.Web/src/test/mocks/handlers.ts covering all consumed endpoints per contracts/api-client.md: GET /health, GET/POST /api/films, GET /api/films/:id, GET /api/customers, GET /api/customers/:id, GET/POST /api/rentals, GET /api/rentals/:id, PUT /api/rentals/:id/return
- [ ] T009 [P] Create test fixture data in src/RentalForge.Web/src/test/fixtures/data.ts with sample PagedResponse objects for films (with FilmListItem array), customers (with CustomerListItem array), and rentals (with RentalListItem array); include sample FilmDetail, CustomerListItem, and RentalDetail objects; all matching type definitions from T006
- [ ] T010 Create QueryClient configuration in src/RentalForge.Web/src/lib/query-client.ts with default options (staleTime, retry settings); export factory function for creating test QueryClients with retry: false
- [ ] T011 Create test render utility wrapper in src/RentalForge.Web/src/test/test-utils.tsx that wraps components in QueryClientProvider (fresh client per test) and MemoryRouter; re-export all @testing-library/react utilities
- [ ] T012 Write tests for centralized API client (RED) in src/RentalForge.Web/src/lib/__tests__/api-client.test.ts — test get\<T\> with query params, post\<T\> with JSON body, put\<T\> with optional body, del with void return; test error handling: 400 parses ValidationProblemDetails into ApiError.errors, 404 throws with status/title, 409 throws with conflict title, 5xx throws with generic title; test base URL from env var
- [ ] T013 Implement centralized API client (GREEN) in src/RentalForge.Web/src/lib/api-client.ts per contracts/api-client.md — typed get/post/put/del methods wrapping fetch with base URL from import.meta.env.VITE_API_BASE_URL, JSON content-type headers, response normalization, ApiError throwing on non-2xx
- [ ] T014 Write tests then implement shared UI components in src/RentalForge.Web/src/components/shared/: error-state.tsx (displays error message + optional retry button), empty-state.tsx (displays configurable message + optional icon), loading-state.tsx (displays skeleton placeholders), load-more.tsx (button that calls onLoadMore callback, shows loading state, hides when hasMore is false)
- [ ] T015 Create app shell: root layout in src/RentalForge.Web/src/app/root-layout.tsx with responsive navigation and Outlet; bottom navigation in src/RentalForge.Web/src/components/layout/bottom-nav.tsx (Home, Browse Films, My Rentals, Profile tabs with icons, visible on mobile); sidebar navigation in src/RentalForge.Web/src/components/layout/sidebar-nav.tsx (same links, visible on desktop md+ breakpoint); use NavLink for active state styling
- [ ] T016 Create route configuration with createBrowserRouter in src/RentalForge.Web/src/app/routes.tsx (root layout wrapping all routes: / index, /films, /films/:id, /customers, /customers/:id, /rentals, /rentals/:id, /rentals/new, /profile, * catch-all); create providers wrapper in src/RentalForge.Web/src/app/providers.tsx (QueryClientProvider + RouterProvider); update src/RentalForge.Web/src/main.tsx to render App with providers; update src/RentalForge.Web/index.html with proper title and meta tags
- [ ] T017 Create placeholder pages: Home dashboard (src/RentalForge.Web/src/pages/home.tsx) with welcome message and quick-access links to Films, Customers, Rentals; Not Found page (src/RentalForge.Web/src/pages/not-found.tsx) with 404 message and link to home; Profile placeholder (src/RentalForge.Web/src/pages/profile.tsx) with message that auth will be added in a future release

**Checkpoint**: Foundation ready — app shell renders with navigation, routes work, API client tested, shared components available. User story implementation can now begin.

---

## Phase 3: User Story 1 — Browse and Search Film Catalog (Priority: P1) MVP

**Goal**: Users can browse a paginated film catalog with search and filters, and view film details

**Independent Test**: Load /films, search for a film, apply category filter, tap a film to see details with actors/categories/language

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T018 [US1] Write tests for film query hooks (RED) in src/RentalForge.Web/src/hooks/__tests__/use-films.test.ts — test useInfiniteFilms: returns first page of films, fetches next page on fetchNextPage, applies search/category/rating/yearFrom/yearTo params, handles empty results, handles API error; test useFilm: returns film detail by ID, handles not found (404)

### Implementation for User Story 1

- [ ] T019 [US1] Implement film query hooks (GREEN) in src/RentalForge.Web/src/hooks/use-films.ts — useInfiniteFilms using useInfiniteQuery with queryOptions factory (queryKey: ['films', params], initialPageParam: 1, getNextPageParam from totalPages), useFilm using useQuery (queryKey: ['films', id]); both use api-client.get\<T\>
- [ ] T020 [P] [US1] Implement film-card component in src/RentalForge.Web/src/components/films/film-card.tsx — Card displaying title, MpaaRating badge, release year, rental rate formatted as currency; entire card links to /films/:id
- [ ] T021 [P] [US1] Implement film-filters component in src/RentalForge.Web/src/components/films/film-filters.tsx — search Input with debounced onChange, category Select dropdown, rating Select dropdown (G/PG/PG-13/R/NC-17), year range Inputs; all filter changes call onFilterChange callback with updated FilmSearchParams
- [ ] T022 [P] [US1] Implement film-detail component in src/RentalForge.Web/src/components/films/film-detail.tsx — displays all FilmDetail attributes: title, description, rating badge, release year, language, rental rate, rental duration, length, replacement cost, special features as badges, actors list, categories list
- [ ] T023 [US1] Implement films list page in src/RentalForge.Web/src/pages/films-list.tsx — wires useInfiniteFilms hook with film-filters for search/filter state, renders film-card grid from data.pages, uses load-more with hasNextPage/fetchNextPage, displays loading-state on initial load, error-state on error, empty-state on no results
- [ ] T024 [US1] Implement film detail page in src/RentalForge.Web/src/pages/film-detail.tsx — reads id from route params, wires useFilm hook to film-detail component, displays loading-state while fetching, error-state on error/not-found, back navigation link to /films

**Checkpoint**: User Story 1 complete — film catalog is fully browsable with search, filters, infinite scroll, and detail views

---

## Phase 4: User Story 2 — View and Search Customers (Priority: P2)

**Goal**: Staff can browse and search the customer list and view customer details

**Independent Test**: Navigate to /customers from Home dashboard link, search for a customer by name, tap to view customer details

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T025 [US2] Write tests for customer query hooks (RED) in src/RentalForge.Web/src/hooks/__tests__/use-customers.test.ts — test useInfiniteCustomers: returns first page, fetches next page, applies search param, handles empty/error; test useCustomer: returns customer by ID, handles not found

### Implementation for User Story 2

- [ ] T026 [US2] Implement customer query hooks (GREEN) in src/RentalForge.Web/src/hooks/use-customers.ts — useInfiniteCustomers using useInfiniteQuery (queryKey: ['customers', { search }]), useCustomer using useQuery (queryKey: ['customers', id])
- [ ] T027 [P] [US2] Implement customer-card component in src/RentalForge.Web/src/components/customers/customer-card.tsx — Card displaying full name, email, active status badge; links to /customers/:id
- [ ] T028 [P] [US2] Implement customer-detail component in src/RentalForge.Web/src/components/customers/customer-detail.tsx — displays all CustomerListItem attributes: name, email, store ID, address ID, active status, create date, last update
- [ ] T029 [US2] Implement customers list page in src/RentalForge.Web/src/pages/customers-list.tsx — search Input with debounced onChange, customer-card grid from useInfiniteCustomers, load-more button, loading/error/empty states
- [ ] T030 [US2] Implement customer detail page in src/RentalForge.Web/src/pages/customer-detail.tsx — reads id from route params, wires useCustomer to customer-detail component, loading/error states, back link to /customers

**Checkpoint**: User Stories 1 AND 2 both work independently — films browsable, customers searchable from Home dashboard

---

## Phase 5: User Story 3 — View Rentals and Process Rental Transactions (Priority: P3)

**Goal**: Staff can view rentals with filters, create new rentals with Zod validation, and process returns

**Independent Test**: Navigate to /rentals, filter by active only, create a new rental at /rentals/new with customer/film/store selection, process a return from the rental list

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T031 [US3] Write tests for rental query and mutation hooks (RED) in src/RentalForge.Web/src/hooks/__tests__/use-rentals.test.ts — test useInfiniteRentals: returns first page, applies customerId/activeOnly filters, fetches next page; test useRental: returns detail by ID; test useCreateRental mutation: posts CreateRentalRequest, invalidates ['rentals'] on success, handles validation errors; test useReturnRental mutation: puts to /return endpoint, invalidates ['rentals'] on success

### Implementation for User Story 3

- [ ] T032 [US3] Implement rental hooks (GREEN) in src/RentalForge.Web/src/hooks/use-rentals.ts — useInfiniteRentals (queryKey: ['rentals', params]), useRental (queryKey: ['rentals', id]), useCreateRental (useMutation calling api-client.post, onSuccess invalidates ['rentals']), useReturnRental (useMutation calling api-client.put for /return, onSuccess invalidates ['rentals'])
- [ ] T033 [P] [US3] Implement rental-card component in src/RentalForge.Web/src/components/rentals/rental-card.tsx — Card displaying rental ID, rental date, return status badge (Active/Returned), customer ID, inventory ID; entire card links to /rentals/:id; "Return" button visible only for active rentals, calls onReturn callback (stops propagation to prevent navigation)
- [ ] T033b [P] [US3] Implement rental-detail component in src/RentalForge.Web/src/components/rentals/rental-detail.tsx — displays all RentalDetail attributes: film title, customer full name (first + last), staff full name (first + last), rental date, return date/status, store ID, inventory ID
- [ ] T034 [P] [US3] Write tests then implement rental-form component with Zod validation in src/RentalForge.Web/src/components/rentals/rental-form.tsx — controlled inputs for customer ID, film ID, store ID, staff ID; submit handler runs createRentalSchema.safeParse() and displays fieldErrors per field; calls onSubmit with validated data; tests verify: required field errors shown when empty, valid data calls onSubmit, API validation errors displayed
- [ ] T035 [US3] Implement rentals list page in src/RentalForge.Web/src/pages/rentals-list.tsx — active-only toggle filter, rental-card grid from useInfiniteRentals, load-more, return action using useReturnRental with success toast (Sonner), loading/error/empty states, link to /rentals/new
- [ ] T036 [US3] Implement create rental page in src/RentalForge.Web/src/pages/rental-new.tsx — wires useCreateRental mutation to rental-form, shows success toast and navigates to /rentals on success, displays API validation errors (e.g., "no available inventory") on failure
- [ ] T036b [US3] Implement rental detail page in src/RentalForge.Web/src/pages/rental-detail.tsx — reads id from route params, wires useRental hook to rental-detail component, loading/error states, back link to /rentals

**Checkpoint**: All three API integrations complete — films browsable, customers searchable, rentals manageable with create and return

---

## Phase 6: User Story 4 — Install Application on Mobile Device (Priority: P4)

**Goal**: Application meets PWA installability criteria with custom icon and standalone display mode

**Independent Test**: Build the app (`npm run build`), serve with `npm run preview`, open on mobile Chrome — verify install prompt appears or "Add to Home Screen" is available; launch installed app and verify standalone mode

- [ ] T037 [US4] Create PWA icon assets in src/RentalForge.Web/public/icons/: icon-192x192.png, icon-512x512.png, icon-maskable-192x192.png, icon-maskable-512x512.png (simple RentalForge branded icons); create src/RentalForge.Web/public/robots.txt
- [ ] T038 [US4] Configure vite-plugin-pwa in src/RentalForge.Web/vite.config.ts: registerType 'autoUpdate', manifest with name 'RentalForge', short_name 'RentalForge', description, start_url '/', display 'standalone', theme_color, background_color, icons array (192/512 regular + maskable); workbox config with globPatterns for static assets, navigateFallback '/index.html'; no runtimeCaching for API routes
- [ ] T038b [US4] Write test in src/RentalForge.Web/src/test/pwa-manifest.test.ts that imports the vite-plugin-pwa manifest config from vite.config.ts and asserts: name, short_name, start_url, display: 'standalone', and icons array contains 192x192 and 512x512 entries (both regular and maskable); validates US4-S1/S2 criteria at the config level

**Checkpoint**: PWA installability criteria met — app installs to home screen with custom icon and launches in standalone mode

---

## Phase 7: User Story 5 — Switch Between Light and Dark Mode (Priority: P5)

**Goal**: Users can toggle between light/dark mode with system preference detection and localStorage persistence

**Independent Test**: Load app — verify it matches device system theme; toggle to dark mode — verify all components switch; close and reopen — verify preference persisted

### Tests for User Story 5

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T039 [US5] Write tests for ThemeProvider and useTheme hook (RED) in src/RentalForge.Web/src/hooks/__tests__/use-theme.test.ts — test: defaults to system preference, setTheme('dark') adds .dark class to documentElement, setTheme('light') removes .dark class, persists preference to localStorage under 'rentalforge-theme' key, reads persisted preference on mount

### Implementation for User Story 5

- [ ] T040 [US5] Implement ThemeProvider context and useTheme hook (GREEN) in src/RentalForge.Web/src/hooks/use-theme.ts — ThemeProvider component managing 'dark' | 'light' | 'system' state, applying/removing .dark class on document.documentElement, reading/writing localStorage, listening to matchMedia('prefers-color-scheme: dark') for system mode; integrate ThemeProvider into src/RentalForge.Web/src/app/providers.tsx
- [ ] T041 [US5] Implement theme-toggle component in src/RentalForge.Web/src/components/layout/theme-toggle.tsx — DropdownMenu with Light/Dark/System options using Sun/Moon icons, calls setTheme() from useTheme; integrate into root layout header in src/RentalForge.Web/src/app/root-layout.tsx

**Checkpoint**: Full dark mode support — system preference detection, manual toggle, persistence across sessions, all components themed

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across all user stories

- [ ] T042 Verify all tests pass by running `npm run test` from src/RentalForge.Web/; fix any failing tests; ensure zero test warnings
- [ ] T043 Verify production build succeeds by running `npm run build` from src/RentalForge.Web/; verify monorepo coexistence by running `dotnet build` and `dotnet test` from repo root — both must pass unaffected
- [ ] T044 Run quickstart.md validation: verify all npm scripts (dev, build, preview, test, test:watch, lint, typecheck) work from src/RentalForge.Web/; verify dev server starts and all pages render at each route; verify no TypeScript errors with `npm run typecheck`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phases 3–7)**: All depend on Foundational phase completion
  - US1 (P1): Can start after Phase 2 — no dependencies on other stories
  - US2 (P2): Can start after Phase 2 — no dependencies on other stories
  - US3 (P3): Can start after Phase 2 — no dependencies on other stories (API client handles all data)
  - US4 (P4): Can start after Phase 2 — no dependencies on other stories (config-only)
  - US5 (P5): Can start after Phase 2 — no dependencies on other stories (theme is cross-cutting)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (Films)**: Independent — foundational only
- **US2 (Customers)**: Independent — foundational only
- **US3 (Rentals)**: Independent — foundational only (rental form uses IDs, not embedded customer/film components)
- **US4 (PWA)**: Independent — only modifies vite.config.ts and adds static assets
- **US5 (Dark Mode)**: Independent — shadcn/ui CSS variables handle dark styling; ThemeProvider just toggles the class

### Within Each User Story

- Tests MUST be written and FAIL before implementation (RED → GREEN)
- Hooks before pages (pages depend on hooks for data)
- Components can be parallel with each other and with hooks (components receive props)
- Pages come last (compose hooks + components)
- Story complete before moving to next priority (when working sequentially)

### Parallel Opportunities

- T003 and T004 can run in parallel (different config files)
- T006, T007, T008, T009 can all run in parallel (different directories)
- Within US1: T020, T021, T022 can run in parallel (different component files)
- Within US2: T027, T028 can run in parallel
- Within US3: T033, T033b, T034 can run in parallel
- All user stories (Phases 3–7) can run in parallel if team capacity allows

---

## Parallel Example: User Story 1

```bash
# After T019 (hooks implemented), launch all components in parallel:
Task: "Implement film-card component in src/RentalForge.Web/src/components/films/film-card.tsx"
Task: "Implement film-filters component in src/RentalForge.Web/src/components/films/film-filters.tsx"
Task: "Implement film-detail component in src/RentalForge.Web/src/components/films/film-detail.tsx"

# Then sequentially compose into pages:
Task: "Implement films list page in src/RentalForge.Web/src/pages/films-list.tsx"
Task: "Implement film detail page in src/RentalForge.Web/src/pages/film-detail.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (Film Catalog)
4. **STOP and VALIDATE**: Browse /films, search, filter, view detail — all working
5. Deploy/demo if ready — the app has real value as a film browser

### Incremental Delivery

1. Setup + Foundational → App shell renders with navigation
2. Add US1 (Films) → Test independently → Film catalog browsable (MVP!)
3. Add US2 (Customers) → Test independently → Customer lookup working
4. Add US3 (Rentals) → Test independently → Full rental workflow
5. Add US4 (PWA) → Test installability → Native-like mobile experience
6. Add US5 (Dark Mode) → Test theme toggle → Polished UX
7. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks in same phase
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- TDD is NON-NEGOTIABLE per constitution — write failing tests before implementation for hooks and API client
- Commit after each completed task or logical group
- Stop at any checkpoint to validate story independently
- All file paths are relative to repository root (src/RentalForge.Web/...)
- shadcn/ui components are added via CLI (npx shadcn@latest add) not manually created
