# Quickstart: 008-react-frontend-scaffold

**Date**: 2026-02-23

## Prerequisites

- Node.js (latest stable LTS)
- npm
- Backend API running (`dotnet run --project src/RentalForge.Api`)

## Setup

```bash
# Navigate to frontend project
cd src/RentalForge.Web

# Install dependencies
npm install

# Copy environment config
cp .env.example .env
# Edit .env to set VITE_API_BASE_URL if backend is not on localhost:5000
```

## Development

```bash
# Start dev server (from src/RentalForge.Web/)
npm run dev

# Run tests
npm run test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:coverage

# Build for production
npm run build

# Preview production build
npm run preview

# Lint
npm run lint

# Type check
npm run typecheck
```

## npm Scripts

| Script | Command | Description |
|--------|---------|-------------|
| `dev` | `vite` | Start Vite dev server |
| `build` | `tsc -b && vite build` | Type-check then build |
| `preview` | `vite preview` | Preview production build |
| `test` | `vitest run` | Run tests once |
| `test:watch` | `vitest` | Run tests in watch mode |
| `test:coverage` | `vitest run --coverage` | Run tests with coverage |
| `lint` | `eslint .` | Lint source files |
| `typecheck` | `tsc -b --noEmit` | Type-check without emit |

## Adding shadcn/ui Components

```bash
# Add individual components as needed
npx shadcn@latest add button
npx shadcn@latest add card
npx shadcn@latest add input
npx shadcn@latest add select
npx shadcn@latest add skeleton
npx shadcn@latest add badge
npx shadcn@latest add dialog
npx shadcn@latest add sonner
```

## Monorepo Coexistence

The frontend project is fully independent from the .NET backend:

- `dotnet build` / `dotnet test` ignore `src/RentalForge.Web/` entirely
- `npm` commands are scoped to `src/RentalForge.Web/package.json`
- No shared build pipeline between frontend and backend
- Both can run concurrently during development (different ports)

## Key File Locations

| Purpose | Path |
|---------|------|
| App entry | `src/main.tsx` |
| Route config | `src/app/routes.tsx` |
| API client | `src/lib/api-client.ts` |
| Query hooks | `src/hooks/use-*.ts` |
| Zod schemas | `src/lib/validators.ts` |
| Theme provider | `src/hooks/use-theme.ts` |
| Test setup | `src/test/setup.ts` |
| MSW handlers | `src/test/mocks/handlers.ts` |
| Vite config | `vite.config.ts` |
| Vitest config | `vitest.config.ts` |
| shadcn config | `components.json` |
| PWA icons | `public/icons/` |
| Environment | `.env` / `.env.example` |
