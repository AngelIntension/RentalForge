# Research: 008-react-frontend-scaffold

**Date**: 2026-02-23
**Branch**: `008-react-frontend-scaffold`

## R1: React 19 + Vite + TypeScript Setup

**Decision**: Use `npm create vite@latest` with `react-ts` template, producing React 19.2.4 + Vite 7.3.1 + TypeScript 5.9.3.

**Rationale**: The official `react-ts` template ships with `strict: true` already enabled plus additional strictness flags (`noUnusedLocals`, `noUnusedParameters`, `erasableSyntaxOnly`, `noUncheckedSideEffectImports`). Uses `@vitejs/plugin-react@^5.1.3` (Babel-based) which is the current recommended default — the SWC plugin was archived July 2025 and the OXC plugin was deprecated.

**Alternatives considered**:
- `@vitejs/plugin-react-swc` — archived, uncertain future
- `@vitejs/plugin-react-oxc` — deprecated
- Next.js / Remix — SSR frameworks; overkill for a client-side SPA per constitution

**Key versions**:

| Package | Version |
|---------|---------|
| react | ^19.2.4 |
| react-dom | ^19.2.4 |
| vite | ^7.3.1 |
| typescript | ~5.9.3 |
| @vitejs/plugin-react | ^5.1.3 |
| @types/react | ^19.2.13 |
| @types/react-dom | ^19.2.3 |

**React 19 notes**: No `import React` needed (`jsx: "react-jsx"`). `propTypes`/`defaultProps` removed from function components. `forwardRef` no longer needed (direct props). `react-dom/test-utils` removed — import `act` from `react` directly.

---

## R2: React Router v7 — Mode Selection

**Decision**: Use React Router v7 in **Data Mode** (library mode with `createBrowserRouter`). Do NOT use Framework Mode or file-based routing.

**Rationale**: React Router v7 has three modes: Declarative, Data, and Framework. File-based routing is **only available in Framework Mode** which requires the `@react-router/dev` Vite plugin and brings SSR/full-stack complexity. For a client-side SPA, Data Mode provides centralized route config with `createBrowserRouter` + `RouterProvider`, optional `loader`/`action` functions, and pending state management — without Framework Mode overhead. `react-router-dom` is deprecated and merged into `react-router` as of v7.

**Alternatives considered**:
- Framework Mode with `@react-router/dev` — overkill for SPA, adds SSR complexity
- Declarative Mode (JSX `<Routes>`) — works but lacks centralized route config and built-in data loading
- TanStack Router — would add another TanStack dependency; React Router is the ecosystem standard

**Key packages**:

| Package | Version | Notes |
|---------|---------|-------|
| react-router | ^7.13.1 | All routing (replaces react-router-dom) |

**Route structure** (Data Mode):
```
/ (root layout — shell with nav + Outlet)
├── index → Home dashboard
├── /films → Films list (infinite scroll)
├── /films/:id → Film detail
├── /customers → Customers list
├── /customers/:id → Customer detail
├── /rentals → Rentals list
├── /rentals/new → Create rental form
└── /profile → Profile placeholder
```

---

## R3: Tailwind CSS v4 + shadcn/ui

**Decision**: Use Tailwind CSS v4.2.0 with `@tailwindcss/vite` plugin and shadcn/ui (latest) with New York style.

**Rationale**: Tailwind v4 replaces JS config with CSS-first `@theme` directives, eliminates PostCSS config, and provides a first-party Vite plugin. shadcn/ui has full v4 support — the CLI initializes v4 projects by default, all components updated for React 19 (no `forwardRef`), and `tailwindcss-animate` is no longer needed (deprecated March 2025). Dark mode uses CSS variables in OKLCH color space with `.dark` class toggle.

**Alternatives considered**:
- Tailwind v3 — legacy, v4 is the current stable with better DX
- Material UI / Chakra UI — heavier runtime, less customizable than shadcn/ui's copy-to-project model
- Radix Themes — shadcn/ui already builds on Radix primitives

**Setup sequence**:
1. `npm install tailwindcss @tailwindcss/vite`
2. Add `tailwindcss()` to Vite plugins
3. `@import "tailwindcss"` in index.css
4. `npx shadcn@latest init` (New York style, neutral base color)
5. Add components individually: `npx shadcn@latest add button card input ...`

**Key shadcn/ui components for this feature**:
- Data display: Card, Badge, Skeleton, Spinner, Empty
- Forms: Input, Select, Label, Button
- Navigation: Tabs (for filters), Breadcrumb
- Feedback: Sonner (toasts), Tooltip
- Layout: Sheet (mobile drawer), Dialog (confirmations)

**Dark mode**: Custom `ThemeProvider` context (not `next-themes`) that manages `dark`/`light`/`system` state, persists to `localStorage`, and toggles `.dark` class on `<html>`. shadcn/ui docs provide the Vite-specific pattern.

**Path alias**: `@/` maps to `./src/` via tsconfig `paths` + Vite `resolve.alias`.

---

## R4: TanStack Query v5

**Decision**: Use TanStack Query v5 (`@tanstack/react-query@^5.90.21`) with `useInfiniteQuery` for all list views and `queryOptions` factories for typed, reusable query definitions.

**Rationale**: TanStack Query v5's `useInfiniteQuery` directly supports the infinite scroll + "Load More" pattern with `fetchNextPage`, `hasNextPage`, and `isFetchingNextPage`. The `queryOptions()` helper provides full TypeScript inference for query keys and return types, enabling the same config in components, prefetching, and cache reads. Hierarchical query keys (`['films']`, `['films', id]`) enable targeted invalidation.

**Alternatives considered**:
- SWR — less mature infinite query support, no `queryOptions` equivalent
- Plain fetch + useState — loses caching, dedup, background refetching, devtools
- RTK Query — heavier, Redux dependency not justified per YAGNI

**Key packages**:

| Package | Version |
|---------|---------|
| @tanstack/react-query | ^5.90.21 |
| @tanstack/react-query-devtools | ^5.91.3 |

**Architecture pattern**:
- `src/lib/api-client.ts` — single fetch wrapper (base URL, headers, error normalization)
- `src/hooks/use-*.ts` — thin hooks wrapping `useQuery`/`useInfiniteQuery`/`useMutation` with `queryOptions` factories
- Query keys: `['films']`, `['films', id]`, `['customers']`, `['customers', id]`, `['rentals']`, etc.
- Mutations use `useMutation` with `onSuccess` invalidation of related query keys

---

## R5: PWA with vite-plugin-pwa

**Decision**: Use `vite-plugin-pwa@^1.2.0` with `registerType: 'autoUpdate'` and `generateSW` strategy for static asset pre-caching only.

**Rationale**: The plugin generates a Workbox service worker and web manifest from Vite config. `autoUpdate` automatically activates new service workers without user prompts (simplest for v1). `generateSW` pre-caches all Vite build output (JS, CSS, HTML) for instant repeat loads. API calls are NOT cached — they pass through to the network, avoiding stale data issues. This meets the spec's "installability + basic static caching, not offline-first" requirement.

**Alternatives considered**:
- `injectManifest` — full control over service worker code, unnecessary for basic caching
- No PWA plugin — wouldn't meet FR-019/FR-020 installability requirements
- Workbox directly — vite-plugin-pwa wraps it with Vite integration

**Manifest requirements for Chrome + Safari installability**:
- `name`, `short_name`, `start_url: '/'`, `display: 'standalone'`
- `theme_color`, `background_color`
- Icons: 192x192 and 512x512 (PNG), plus maskable variant

---

## R6: Vitest + React Testing Library

**Decision**: Use Vitest 4.0.18 with `@testing-library/react@^16.3.2`, jsdom environment, and MSW for API mocking in integration tests.

**Rationale**: Vitest 4 integrates natively with Vite's config and plugin system. React Testing Library is the constitution-mandated frontend testing library. jsdom provides the browser DOM environment. MSW intercepts fetch at the network level, testing the full query path through the API client.

**Alternatives considered**:
- Jest — requires separate Babel/SWC config, doesn't share Vite's transform pipeline
- Happy DOM — faster but less compatible; jsdom is the standard
- `vi.mock` for all API mocks — simpler but doesn't test the API client's error handling path

**Key packages**:

| Package | Version | Notes |
|---------|---------|-------|
| vitest | ^4.0.18 | Test runner |
| @testing-library/react | ^16.3.2 | Component testing |
| @testing-library/jest-dom | ^6.9.1 | DOM matchers (use `/vitest` entry) |
| @testing-library/user-event | ^14.6.1 | User interaction simulation |
| msw | ^2.x | Network-level API mocking |

**jsdom note**: Pin to `^26.1.0` — jsdom 27.x has ESM compatibility issues with Vitest 4 (vitest-dev/vitest#9279).

**TanStack Query test pattern**: Create fresh `QueryClient` per test with `retry: false`. Wrap components in `QueryClientProvider` via test utility wrapper.

---

## R7: Zod for Client-Side Validation

**Decision**: Use Zod v4 (`zod@^4.3.6`) directly — no react-hook-form, no resolver packages.

**Rationale**: Only one form exists in this feature (Create Rental with 4 integer fields). `react-hook-form` + `@hookform/resolvers` adds two unnecessary dependencies for a single simple form. The plain pattern is: `z.object` schema → `safeParse()` in submit handler → `error.flatten().fieldErrors` to state → render errors per field. `z.infer<typeof schema>` derives TypeScript types from the schema.

**Alternatives considered**:
- react-hook-form + Zod resolver — overkill for one 4-field form per YAGNI
- Yup — Zod has better TypeScript inference and is faster in v4
- No validation library — would miss `z.infer` type generation benefit

**Schema pattern**:
```
z.object({
  filmId: z.coerce.number().int().positive(),
  storeId: z.coerce.number().int().positive(),
  customerId: z.coerce.number().int().positive(),
  staffId: z.coerce.number().int().positive(),
})
```
