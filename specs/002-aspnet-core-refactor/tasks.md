# Tasks: ASP.NET Core Controller Refactor

**Input**: Design documents from `/specs/002-aspnet-core-refactor/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/health.md

**Tests**: Existing integration tests cover runtime acceptance scenarios. One new test added for Swagger metadata verification (constitution II: every acceptance scenario MUST map to an automated test). Verification tasks confirm all tests pass.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create new directory structure for controller-based architecture

- [x] T001 Create `Controllers/` and `Models/` directories under `src/RentalForge.Api/`

**Checkpoint**: Directory structure ready for controller and model files

---

## Phase 2: Foundational (Program.cs Migration)

**Purpose**: Reconfigure application startup for controller-based routing. This MUST be complete before controller implementation.

**CRITICAL**: After this phase, existing tests will fail (Red) because `MapHealthEndpoint()` is removed but no controller exists yet. This is expected and follows the Red-Green cycle.

- [x] T002 Update `src/RentalForge.Api/Program.cs` — replace `AddEndpointsApiExplorer()` with `AddControllers()`, replace `app.MapHealthEndpoint()` with `app.MapControllers()`, remove `using RentalForge.Api.Endpoints`, keep `AddSwaggerGen()` and Swagger UI middleware unchanged (research.md R1, R3)

**Checkpoint**: Program.cs uses controller routing. App builds and starts (edge case: no controllers discovered = no endpoints, app still runs). Tests expected to fail.

---

## Phase 3: User Story 1 - Controller-Based Health Endpoint (Priority: P1) MVP

**Goal**: Migrate the `/health` endpoint from a minimal API extension method to a controller action with identical behavior, response contract, and OpenAPI metadata.

**Independent Test**: `GET /health` returns identical 200/503 responses; Swagger UI shows same operation metadata; all 3 existing integration tests pass.

### Implementation for User Story 1

- [x] T003 [P] [US1] Create `HealthResponse` record in `src/RentalForge.Api/Models/HealthResponse.cs` — extract from `Endpoints/HealthEndpoint.cs`, change namespace to `RentalForge.Api.Models`, preserve exact record shape (FR-007, data-model.md)
- [x] T004 [US1] Create `HealthController` in `src/RentalForge.Api/Controllers/HealthController.cs` — `[ApiController]`, `[Route("")]`, `[HttpGet("health")]` action with health check logic migrated from `HealthEndpoint.MapHealthEndpoint`, OpenAPI attributes per research.md R2/R5: `[SwaggerOperation(OperationId = "HealthCheck", Summary = "Database health check")]`, `[ProducesResponseType(typeof(HealthResponse), 200)]`, `[ProducesResponseType(typeof(HealthResponse), 503)]`, `[SwaggerResponse(200, "Database is healthy and reachable")]`, `[SwaggerResponse(503, "Database is unhealthy or unreachable")]`. Include XML `<summary>` documentation on the controller class and the health check action method per constitution V (FR-001, FR-002, FR-005, contracts/health.md)
- [x] T005 [US1] Add Swagger metadata integration test in `tests/RentalForge.Api.Tests/Integration/HealthEndpointTests.cs` — fetch `/swagger/v1/swagger.json`, parse JSON, assert the `/health` GET operation has OperationId "HealthCheck", summary "Database health check", 200 description "Database is healthy and reachable", 503 description "Database is unhealthy or unreachable" (SC-003, constitution II: every acceptance scenario MUST map to an automated test)
- [x] T006 [US1] Run `dotnet test` — verify all 4 integration tests pass: `HealthEndpoint_ReturnsOk_WhenDatabaseIsReachable`, `HealthEndpoint_Returns503_WhenDatabaseIsUnreachable`, `App_FailsFast_WhenConnectionStringMissing`, plus new Swagger metadata test (SC-001, SC-002, SC-003)

**Checkpoint**: Health endpoint fully functional via controller. All tests pass (Green). Swagger metadata matches pre-refactor baseline. MVP complete.

---

## Phase 4: User Story 2 - ASP.NET Core Controller Infrastructure (Priority: P2)

**Goal**: Remove all minimal API artifacts and verify the codebase is fully migrated to controller-based routing.

**Independent Test**: `Endpoints/` directory does not exist; no `app.Map*` calls for business endpoints in Program.cs; `dotnet build` succeeds with zero errors.

### Implementation for User Story 2

- [x] T007 [US2] Delete `src/RentalForge.Api/Endpoints/` directory and all its contents (`HealthEndpoint.cs`) (FR-004)
- [x] T008 [US2] Run `dotnet build` — verify zero compilation errors with `Endpoints/` removed (confirms no remaining references to deleted code)
- [x] T009 [US2] Verify no minimal API endpoint registrations (`app.MapGet`, `app.MapPost`, `app.MapPut`, `app.MapDelete`) remain for business endpoints in `src/RentalForge.Api/Program.cs` (SC-004)

**Checkpoint**: Codebase fully migrated. No minimal API artifacts remain. Build clean.

---

## Phase 5: Polish & Final Validation

**Purpose**: End-to-end validation across all acceptance scenarios

- [x] T010 Run full quickstart.md verification checklist from `specs/002-aspnet-core-refactor/quickstart.md` — build, run, curl, Swagger UI, test suite
- [x] T011 [P] Run `dotnet test` — final confirmation all 4 tests pass after complete refactor (SC-001)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (directories must exist)
- **User Story 1 (Phase 3)**: Depends on Phase 2 (controller routing must be active in Program.cs)
- **User Story 2 (Phase 4)**: Depends on Phase 3 (controller must exist before deleting old endpoint code)
- **Polish (Phase 5)**: Depends on Phase 4 (all cleanup must be done before final validation)

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational (Phase 2). No dependency on US2.
- **User Story 2 (P2)**: Depends on US1 completion (cannot delete old code until new controller is verified working).

### Within Each User Story

- T003 (model) before T004 (controller) — controller references HealthResponse
- T004 (controller) before T005 (Swagger test) — test references controller metadata
- T005 (Swagger test) before T006 (test verification) — all tests must exist before running suite
- T007 (delete) before T008 (build verification) — verify clean build after deletion
- T008 (build) before T009 (pattern scan) — confirm compilation before code scan

### Parallel Opportunities

- T003 is marked [P] — `Models/HealthResponse.cs` touches a different file than `Program.cs` (T002). In practice, these are sequential due to phase ordering.
- T010 and T011 can run in parallel (quickstart checklist vs test suite).

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001)
2. Complete Phase 2: Foundational (T002) — tests now fail (Red)
3. Complete Phase 3: User Story 1 (T003–T006) — tests pass (Green)
4. **STOP and VALIDATE**: All 4 integration tests pass, Swagger metadata correct
5. MVP delivered — health endpoint works via controller

### Full Delivery

1. Complete MVP above
2. Complete Phase 4: User Story 2 (T007–T009) — cleanup
3. Complete Phase 5: Polish (T010–T011) — final validation
4. Feature complete — ready for PR

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- One new test added (Swagger metadata verification) per constitution II; existing 3 integration tests are behavior-based and agnostic to implementation pattern (research.md R6)
- The Red-Green cycle occurs naturally: Phase 2 breaks tests (Red), Phase 3 restores them (Green)
- Commit after each phase or logical task group
- Stop at Phase 3 checkpoint to validate MVP independently
