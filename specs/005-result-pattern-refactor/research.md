# Research: Result Pattern Refactor

**Date**: 2026-02-22
**Feature**: [spec.md](spec.md) | [plan.md](plan.md)

## R1: Result Pattern Library Selection

**Decision**: Ardalis.Result 10.1.0 with companion packages.

**Rationale**: Built-in `ValidationError(identifier, errorMessage)` type maps
directly to field-level validation errors. First-class FluentValidation bridge
(`.AsErrors()`) converts `ValidationResult` to `List<ValidationError>` with
zero manual mapping. Production-ready ASP.NET Core package (v10.1.0) with
HTTP-semantic `ResultStatus` enum (Ok, Created, NoContent, Invalid, NotFound,
etc.). Wide adoption (>56M NuGet downloads).

**Alternatives considered**:
- **FluentResults 4.0.0**: No built-in ValidationError type (uses generic
  `IError`). No FluentValidation bridge package. ASP.NET Core extension stuck
  at v0.2.0 preview. Would require custom adapter code.
- **OneOf / ErrorOr**: Discriminated union style, less HTTP-semantic. No
  built-in validation error aggregation. Would require custom plumbing for
  ASP.NET Core integration.
- **Custom Result type**: Violates YAGNI and constitution dependency policy
  (prefer standard library or validated packages).

## R2: Ardalis.Result API Surface (v10.1.0)

### Core Types

- `Result<T>`: Generic result wrapping a value of type `T`.
- `Result`: Non-generic result (no value — for void operations like delete).
- `ResultStatus` enum: `Ok`, `Created`, `NoContent`, `Invalid`, `NotFound`,
  `Error`, `Forbidden`, `Unauthorized`, `Conflict`, `CriticalError`, `Unavailable`.
- `ValidationError`: Class with `Identifier` (field name), `ErrorMessage`,
  optional `ErrorCode`, optional `Severity`.

### Factory Methods Used in This Refactoring

| Factory | Status | Use Case |
|---------|--------|----------|
| `Result<T>.Success(value)` | `Ok` | Successful read/update |
| `Result<T>.Created(value)` | `Created` | Successful creation |
| `Result<T>.Invalid(errors)` | `Invalid` | Validation failures |
| `Result<T>.NotFound()` | `NotFound` | Resource not found or inactive |
| `Result.NoContent()` | `NoContent` | Successful deactivation |
| `Result.NotFound()` | `NotFound` | Deactivation of missing resource |

### FluentValidation Bridge (`.AsErrors()`)

Extension method on `FluentValidation.Results.ValidationResult`. Converts each
`ValidationFailure` to a `ValidationError`:

| `ValidationFailure` | `ValidationError` |
|---------------------|-------------------|
| `.PropertyName` | `.Identifier` |
| `.ErrorMessage` | `.ErrorMessage` |
| `.ErrorCode` | `.ErrorCode` |
| `.Severity` | `.Severity` (mapped) |

Usage: `var errors = (await validator.ValidateAsync(request)).AsErrors();`
Returns `List<ValidationError>` — feed directly to `Result<T>.Invalid(errors)`.

## R3: Controller Translation Strategy

**Decision**: Explicit `result.Status` switch expressions (not
`[TranslateResultToActionResult]` attribute).

**Rationale**:
- `[TranslateResultToActionResult]` uses `Created(uri, value)` for 201
  responses. To generate a proper Location header, the service would need
  to know URL paths — violating Clean Architecture (Principle III).
- The attribute maps NotFound to `ProblemDetails` body. Existing tests
  expect a bare 404 with no body. Manual mapping preserves this.
- The attribute requires controller actions to return `Result<T>` directly
  (not `ActionResult<T>`), which limits flexibility.
- Only 5 actions in one controller — the switch expression is 3–5 lines each.

**Invalid → ValidationProblemDetails conversion**:
```
foreach (var error in result.ValidationErrors)
    ModelState.AddModelError(error.Identifier, error.ErrorMessage);
return ValidationProblem(ModelState);
```
This produces identical JSON to the current `ServiceValidationException`
handler because both use `ModelState` → `ValidationProblemDetails`.

## R4: FluentValidation Integration Point

**Decision**: Move validation from ASP.NET pipeline auto-validation into the
service layer.

**Rationale**:
- FR-002 requires aggregating all validation errors (input + FK existence)
  into a single failure result. With auto-validation at the pipeline level,
  FluentValidation errors short-circuit before the service runs — making
  aggregation impossible.
- FR-007 mandates using the `.AsErrors()` bridge, which requires calling
  the validator explicitly in the service.
- `AddFluentValidationAutoValidation()` is deprecated in FluentValidation
  11.x; manual invocation is the recommended pattern.

**Changes**:
1. Remove `builder.Services.AddFluentValidationAutoValidation()` from
   `Program.cs`.
2. Keep `builder.Services.AddValidatorsFromAssemblyContaining<>()` for
   DI registration.
3. Inject `IValidator<CreateCustomerRequest>` and
   `IValidator<UpdateCustomerRequest>` into `CustomerService`.
4. Call `validator.ValidateAsync()` at the start of Create/Update methods.
5. Use `.AsErrors()` to convert to `List<ValidationError>`.
6. Aggregate with FK `ValidationError` instances before returning.

**Test impact**: None. The response format (`ValidationProblemDetails` with
`errors` dictionary) is identical. Error identifiers match because
`.AsErrors()` uses the same `PropertyName` as auto-validation.

## R5: NuGet Package Compatibility

| Package | Version | Targets | .NET 10 Compatible |
|---------|---------|---------|-------------------|
| Ardalis.Result | 10.1.0 | netstandard2.0, net6-8 | Yes (netstandard2.0) |
| Ardalis.Result.AspNetCore | 10.1.0 | net6-8 | Yes (forward-compatible) |
| Ardalis.Result.FluentValidation | 10.1.0 | netstandard2.0, net6-8 | Yes (netstandard2.0) |

All three packages are forward-compatible with .NET 10. The AspNetCore package
uses `<FrameworkReference Include="Microsoft.AspNetCore.App" />` which resolves
against the host framework version.
