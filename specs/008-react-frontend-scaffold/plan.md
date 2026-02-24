# Implementation Plan: React Frontend Scaffold

**Branch**: `008-react-frontend-scaffold` | **Date**: 2026-02-23 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/008-react-frontend-scaffold/spec.md`

## Summary

Scaffold a React 19 + TypeScript (strict mode) SPA frontend under `src/RentalForge.Web` in the existing monorepo. Uses Vite 7 as build tool, React Router v7 (Data Mode) for client-side routing, TanStack Query v5 for API communication with infinite scroll pagination, Zod v4 for form validation, Tailwind CSS v4 + shadcn/ui for mobile-first responsive UI with dark mode, and vite-plugin-pwa for installability. Placeholder pages demonstrate the three existing backend APIs (Films, Customers, Rentals). Tested with Vitest 4 + React Testing Library.

## Technical Context

**Language/Version**: TypeScript 5.9.3 (strict mode) + React 19.2.4
**Primary Dependencies**: Vite 7.3.1, React Router 7.13.1, TanStack Query 5.90.x, Zod 4.3.x, Tailwind CSS 4.2.0, shadcn/ui (latest)
**Storage**: Browser localStorage (theme preference only); all data via backend API
**Testing**: Vitest 4.0.x + React Testing Library 16.3.x + MSW 2.x
**Target Platform**: Modern browsers (Chrome, Firefox, Safari, Edge); mobile-first responsive; PWA installable
**Project Type**: Single Page Application (SPA) — frontend module in monorepo
**Performance Goals**: First meaningful paint < 3s on 3G; search results < 5s
**Constraints**: No SSR; no offline-first data; no auth (deferred to #009); coexist with dotnet backend in monorepo
**Scale/Scope**: 8 routes, 3 API integrations, 1 form, ~15 components

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Spec-Driven Development | PASS | Spec approved, clarifications complete, plan follows spec |
| II. Test-First (NON-NEGOTIABLE) | PASS | Vitest + RTL for frontend tests; TDD workflow planned |
| III. Clean Architecture | PASS | SPA communicates exclusively via REST API; centralized API client layer; no business logic in frontend |
| IV. YAGNI and Simplicity | PASS | No auth pages, no advanced forms, no offline-first, no react-hook-form; minimal dependencies justified |
| V. Observability and Maintainability | PASS | React/TS community naming conventions; structured error handling; TanStack Query devtools |
| VI. Functional Style and Immutability | PASS | React functional components; immutable state via useState/useReducer; no class components |
| Frontend (React SPA) | PASS | TypeScript strict mode; SPA via REST API only; centralized API client; no frontend business logic |
| Testing | PASS | React Testing Library + Vitest per constitution; MSW for API mocking |
| Dependency Policy | PASS | All deps via npm; each justified in research.md |

**Post-Phase 1 re-check**: All gates pass. No violations requiring Complexity Tracking entries.

## Project Structure

### Documentation (this feature)

```text
specs/008-react-frontend-scaffold/
├── plan.md              # This file
├── research.md          # Phase 0 output — technology decisions
├── data-model.md        # Phase 1 output — frontend type definitions
├── quickstart.md        # Phase 1 output — dev setup guide
├── contracts/           # Phase 1 output — API client interface
│   └── api-client.md    # Centralized API client contract
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/RentalForge.Web/
├── public/
│   ├── icons/                    # PWA icons (192x192, 512x512, maskable)
│   └── robots.txt
├── src/
│   ├── app/
│   │   ├── routes.tsx            # React Router Data Mode route config
│   │   ├── root-layout.tsx       # Shell layout (nav + Outlet)
│   │   └── providers.tsx         # QueryClient + Theme + Router providers
│   ├── components/
│   │   ├── ui/                   # shadcn/ui components (CLI-managed)
│   │   ├── layout/
│   │   │   ├── bottom-nav.tsx    # Mobile bottom navigation
│   │   │   ├── sidebar-nav.tsx   # Desktop sidebar navigation
│   │   │   └── theme-toggle.tsx  # Dark/light mode toggle
│   │   ├── shared/
│   │   │   ├── error-state.tsx   # Error display component
│   │   │   ├── empty-state.tsx   # Empty results component
│   │   │   ├── loading-state.tsx # Loading skeleton component
│   │   │   └── load-more.tsx     # Load More button for infinite scroll
│   │   ├── films/
│   │   │   ├── film-card.tsx     # Film list item card
│   │   │   ├── film-filters.tsx  # Category/rating/year filters
│   │   │   └── film-detail.tsx   # Film detail display
│   │   ├── customers/
│   │   │   ├── customer-card.tsx # Customer list item
│   │   │   └── customer-detail.tsx
│   │   └── rentals/
│   │       ├── rental-card.tsx   # Rental list item
│   │       ├── rental-detail.tsx # Rental detail display
│   │       └── rental-form.tsx   # Create rental form
│   ├── hooks/
│   │   ├── use-theme.ts          # Theme context hook
│   │   ├── use-films.ts          # Film query hooks (list, detail, infinite)
│   │   ├── use-customers.ts      # Customer query hooks
│   │   └── use-rentals.ts        # Rental query + mutation hooks
│   ├── lib/
│   │   ├── api-client.ts         # Centralized fetch wrapper
│   │   ├── query-client.ts       # TanStack QueryClient config
│   │   ├── utils.ts              # cn() utility (shadcn)
│   │   └── validators.ts         # Zod schemas
│   ├── pages/
│   │   ├── home.tsx              # Home dashboard
│   │   ├── films-list.tsx        # Films catalog (infinite scroll + filters)
│   │   ├── film-detail.tsx       # Film detail page
│   │   ├── customers-list.tsx    # Customer list (infinite scroll + search)
│   │   ├── customer-detail.tsx   # Customer detail page
│   │   ├── rentals-list.tsx      # Rentals list (infinite scroll + filters)
│   │   ├── rental-new.tsx        # Create rental page
│   │   ├── profile.tsx           # Placeholder profile page
│   │   └── not-found.tsx         # 404 page
│   ├── types/
│   │   ├── film.ts               # Film DTOs (list + detail)
│   │   ├── customer.ts           # Customer DTOs
│   │   ├── rental.ts             # Rental DTOs (list + detail + create request)
│   │   └── api.ts                # Shared API types (PagedResponse, error shapes)
│   ├── test/
│   │   ├── setup.ts              # Vitest setup (jest-dom, MSW server)
│   │   ├── test-utils.tsx        # Render wrapper (QueryClient + Router + Theme)
│   │   ├── mocks/
│   │   │   ├── handlers.ts       # MSW request handlers
│   │   │   └── server.ts         # MSW server setup
│   │   └── fixtures/
│   │       └── data.ts           # Test fixture data
│   ├── main.tsx                  # App entry point
│   └── index.css                 # Tailwind import + shadcn theme variables
├── index.html                    # Vite HTML entry
├── vite.config.ts                # Vite + React + Tailwind + PWA config
├── vitest.config.ts              # Vitest config (jsdom, setup files)
├── tsconfig.json                 # Root TS config (references)
├── tsconfig.app.json             # App TS config (strict mode)
├── tsconfig.node.json            # Node TS config (vite.config)
├── components.json               # shadcn/ui CLI config
├── package.json                  # npm scripts + dependencies
├── eslint.config.js              # ESLint flat config
└── .env.example                  # API base URL placeholder
```

**Structure Decision**: The frontend lives under `src/RentalForge.Web/` following the monorepo convention established by `src/RentalForge.Api/`. All frontend source code is under `src/RentalForge.Web/src/` with separation into `app/` (routing/providers), `components/` (UI), `hooks/` (data fetching), `lib/` (utilities), `pages/` (route-level components), `types/` (TypeScript interfaces), and `test/` (testing infrastructure). This coexists cleanly with the .NET backend — `dotnet build` ignores the Web directory and `npm` commands are scoped to `src/RentalForge.Web/`.

## Complexity Tracking

No violations. All technology choices are justified by concrete present-day needs documented in research.md.
