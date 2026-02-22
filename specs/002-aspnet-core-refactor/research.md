# Research: ASP.NET Core Controller Refactor

**Branch**: `002-aspnet-core-refactor` | **Date**: 2026-02-21

## R1: Controller Registration in ASP.NET Core

**Decision**: Use `builder.Services.AddControllers()` and
`app.MapControllers()` to enable controller-based routing.

**Rationale**: `AddControllers()` is the standard ASP.NET Core
service registration for controller discovery. It adds MVC
controller services without view support (which `AddMvc()` would
add). `MapControllers()` maps attribute-routed controllers to the
endpoint routing pipeline. This pair replaces
`AddEndpointsApiExplorer()` for controller scenarios, though
`AddEndpointsApiExplorer()` can coexist if needed by Swashbuckle.

**Alternatives considered**:
- `AddMvc()`: Adds view engine support we don't need. Rejected
  per YAGNI.
- `AddControllersWithViews()`: Same issue — includes Razor view
  support. Rejected.

## R2: OpenAPI Metadata Migration

**Decision**: Use Swashbuckle attributes (`[SwaggerOperation]`,
`[ProducesResponseType]`, `[SwaggerResponse]`) and ASP.NET Core
attributes (`[Produces]`) to replicate the current OpenAPI metadata.

**Rationale**: The current minimal API uses fluent methods
(`.WithName()`, `.WithSummary()`, `.Produces<>()`,
`.WithOpenApi()`) to configure OpenAPI metadata. Controllers use
attribute-based decoration instead. Swashbuckle.AspNetCore 10.1.4
(already a dependency) provides `[SwaggerOperation]` for
operation names and summaries, and `[SwaggerResponse]` for
response descriptions.

**Mapping**:

| Minimal API                        | Controller Attribute                              |
|------------------------------------|---------------------------------------------------|
| `.WithName("HealthCheck")`         | `[SwaggerOperation(OperationId = "HealthCheck")]` |
| `.WithSummary("Database health…")` | `[SwaggerOperation(Summary = "Database health…")]`|
| `.Produces<T>(200)`                | `[ProducesResponseType(typeof(T), 200)]`          |
| `.WithOpenApi(op => ...)`          | `[SwaggerResponse(200, "Description")]`           |

**Alternatives considered**:
- `Microsoft.AspNetCore.OpenApi` endpoint filters for
  controllers: Not needed; Swashbuckle attributes provide the
  same metadata with less ceremony.
- Manual `IOperationFilter`: Over-engineered for a single
  endpoint. Rejected per YAGNI.

## R3: Swagger Service Registration for Controllers

**Decision**: Replace `AddEndpointsApiExplorer()` with
`AddControllers()` and keep `AddSwaggerGen()`. Swashbuckle
discovers controller endpoints automatically when controllers
are registered.

**Rationale**: `AddEndpointsApiExplorer()` is specifically for
minimal API endpoint discovery. With controllers, Swashbuckle
uses the standard MVC API explorer provided by
`AddControllers()`. The call to `AddEndpointsApiExplorer()` can
be removed.

**Alternatives considered**:
- Keep `AddEndpointsApiExplorer()` alongside `AddControllers()`:
  Technically works but is redundant when no minimal API
  endpoints exist. Rejected to keep startup clean.

## R4: HealthResponse DTO Location

**Decision**: Move `HealthResponse` record to
`Models/HealthResponse.cs` in its own file under a new
`RentalForge.Api.Models` namespace.

**Rationale**: The HealthResponse record is currently co-located
with the minimal API extension method in
`Endpoints/HealthEndpoint.cs`. Since the `Endpoints/` directory
is being deleted, the DTO needs a new home. A `Models/` directory
follows standard ASP.NET Core conventions for response/request
DTOs. The controller will reference it via the `Models` namespace.

**Alternatives considered**:
- Inline in `HealthController.cs`: Violates single-responsibility
  and makes the DTO harder to discover for tests or future
  consumers. Rejected.
- `Contracts/` directory: Reasonable but `Models/` is more
  idiomatic in ASP.NET Core projects. Rejected for consistency.

## R5: Route Configuration

**Decision**: Use `[Route("")]` on the controller class and
`[HttpGet("health")]` on the action to produce `GET /health`.

**Rationale**: The existing endpoint is at the root path `/health`
(no prefix). Using an empty route on the controller and `"health"`
on the action preserves the exact URL. `[ApiController]` attribute
enables automatic model binding and problem details responses.

**Alternatives considered**:
- `[Route("health")]` on class + `[HttpGet]` on action: Also
  produces `/health`. Functionally equivalent, but putting the
  path on the action is more explicit when the controller may
  later host multiple related endpoints (e.g., `/health`,
  `/health/db`, `/health/ready`).
- `[Route("[controller]")]` convention: Would produce
  `/health` only if the class is named `HealthController`.
  Works but is implicit. Rejected for explicitness.

## R6: Test Compatibility

**Decision**: Existing integration tests require no changes.

**Rationale**: `HealthEndpointTests` uses `HttpClient` to make
HTTP requests to `/health` and asserts on status codes and JSON
response bodies. This is behavior-based testing — the tests are
agnostic to whether the endpoint is implemented as a minimal API
or a controller. `WebApplicationFactory<Program>` discovers
controllers automatically when `AddControllers()` is registered
in `Program.cs`. `TestWebAppFactory` overrides only the
`DbContext` registration, which is unaffected by the controller
migration.

**Alternatives considered**: None — no changes needed.

## R7: No New NuGet Packages Required

**Decision**: No new packages needed. All required functionality
is provided by existing dependencies.

**Rationale**:
- `AddControllers()` / `MapControllers()`: Built into ASP.NET
  Core (Microsoft.NET.Sdk.Web).
- `[ApiController]`, `[Route]`, `[HttpGet]`,
  `[ProducesResponseType]`: Built into ASP.NET Core MVC.
- `[SwaggerOperation]`, `[SwaggerResponse]`:
  Swashbuckle.AspNetCore 10.1.4 (already referenced).
- `ControllerBase`: Built into ASP.NET Core MVC.

**Alternatives considered**: None — no gaps identified.
