# Quickstart: Result Pattern Refactor Verification

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

## Prerequisites

- .NET 10.0 SDK installed
- Docker running (for Testcontainers)

## Verification Steps

### 1. Build Verification

```bash
dotnet build
```

Expected: Build succeeds with zero errors and zero warnings related to
Result types or removed exception references.

### 2. Full Test Suite

```bash
dotnet test
```

Expected: All 90 tests pass (38 unit + 30 customer integration + 22 existing).
Zero test modifications — the refactoring is behavior-preserving.

### 3. Key Integration Tests to Watch

These tests exercise the exact code paths being refactored:

| Test | What It Verifies |
|------|-----------------|
| `CreateCustomer_ValidRequest_Returns201WithLocationHeader` | 201 + Location header from `Result<T>.Created` |
| `CreateCustomer_BothStoreIdAndAddressIdInvalid_ReturnsBothErrors` | FK error aggregation via `Result<T>.Invalid` |
| `CreateCustomer_MissingRequiredFields_Returns400` | FluentValidation in service via `.AsErrors()` |
| `UpdateCustomer_NonExistentCustomer_Returns404` | Not-found via `Result<T>.NotFound()` |
| `UpdateCustomer_BothStoreIdAndAddressIdInvalid_ReturnsBothErrors` | FK error aggregation on update |
| `DeactivateCustomer_ActiveCustomer_Returns204` | NoContent via `Result.NoContent()` |
| `DeactivateCustomer_NonExistent_Returns404` | Not-found via `Result.NotFound()` |
| `GetCustomerById_NonExistent_Returns404` | Not-found via `Result<T>.NotFound()` |

### 4. Cleanup Verification

```bash
grep -r "ServiceValidationException" src/ tests/
```

Expected: Zero matches — the legacy exception class and all references are removed.

### 5. Documentation Verification

Check `CLAUDE.md` for:
- [ ] Ardalis.Result 10.1.0 listed in Active Technologies > Backend
- [ ] Ardalis.Result.AspNetCore 10.1.0 listed
- [ ] Ardalis.Result.FluentValidation 10.1.0 listed
- [ ] Key Constraints mentions Result types for expected outcomes
