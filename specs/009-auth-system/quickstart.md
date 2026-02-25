# Quickstart: Authentication System

**Feature Branch**: `009-auth-system`

## Prerequisites

- .NET 10.0 SDK
- Node.js (LTS) + npm
- PostgreSQL 18 running at localhost:5432
- `dvdrental` database loaded
- User secrets configured (see below)

## New User Secrets

After adding the Identity and JWT packages, configure these secrets:

```bash
cd src/RentalForge.Api

# JWT signing key (minimum 256 bits / 32 bytes for HMAC-SHA256)
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"
dotnet user-secrets set "Jwt:Issuer" "RentalForge"
dotnet user-secrets set "Jwt:Audience" "RentalForge"
dotnet user-secrets set "Jwt:AccessTokenExpirationMinutes" "15"
dotnet user-secrets set "Jwt:RefreshTokenExpirationDays" "7"
```

The existing `ConnectionStrings:Dvdrental` secret remains unchanged.

## New NuGet Packages

```bash
cd src/RentalForge.Api
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 10.0.3
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.3
```

## Database Migration

After modifying `DvdrentalContext` to inherit from `IdentityDbContext`:

```bash
cd src/RentalForge.Api
dotnet ef migrations add AddIdentitySchema
dotnet ef database update
```

This creates the `identity` schema with all Identity tables + `refresh_tokens`.

## Seed Default Users

```bash
dotnet run --project src/RentalForge.Api -- --seed
```

Seeds three users (if not already present):
- `admin@rentalforge.dev` (Admin)
- `staff@rentalforge.dev` (Staff)
- `customer@rentalforge.dev` (Customer, linked to existing dvdrental Customer)

Password for all: `DevP@ss1`

## Verify

```bash
# Register a new customer
curl -X POST http://localhost:5062/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"TestP@ss1"}'

# Login
curl -X POST http://localhost:5062/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"staff@rentalforge.dev","password":"DevP@ss1"}'

# Use the returned token
curl http://localhost:5062/api/auth/me \
  -H "Authorization: Bearer <token>"
```

## Run Tests

```bash
# Backend
dotnet test

# Frontend
cd src/RentalForge.Web
npm test
```

## Key Files (after implementation)

### Backend
- `src/RentalForge.Api/Data/DvdrentalContext.cs` — updated to inherit IdentityDbContext
- `src/RentalForge.Api/Data/Entities/ApplicationUser.cs` — custom Identity user
- `src/RentalForge.Api/Data/Entities/RefreshToken.cs` — refresh token entity
- `src/RentalForge.Api/Services/IAuthService.cs` — auth service interface
- `src/RentalForge.Api/Services/AuthService.cs` — auth service implementation
- `src/RentalForge.Api/Controllers/AuthController.cs` — auth endpoints
- `src/RentalForge.Api/Models/Auth/` — request/response DTOs

### Frontend
- `src/RentalForge.Web/src/hooks/use-auth.tsx` — AuthContext + useAuth hook
- `src/RentalForge.Web/src/components/auth/protected-route.tsx` — route guard
- `src/RentalForge.Web/src/pages/login.tsx` — login page
- `src/RentalForge.Web/src/pages/register.tsx` — registration page
- `src/RentalForge.Web/src/lib/api-client.ts` — updated with auth header
