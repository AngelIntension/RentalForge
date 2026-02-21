<!--
  Sync Impact Report
  ==================
  Version change: 1.1.0 → 1.2.0 (MINOR — Technology Stack materially expanded)
  Modified principles: none
  Added sections: none (existing section expanded)
  Removed sections: none
  Changes:
    - Technology Stack: added Database, ORM, Test Infrastructure,
      and API Documentation subsections with EF Core, Npgsql,
      PostgreSQL (dvdrental), Testcontainers, and Swagger/OpenAPI
  Templates requiring updates:
    - .specify/templates/plan-template.md — ✅ no updates needed
      (Technical Context Storage/Testing fields are dynamic)
    - .specify/templates/spec-template.md — ✅ no updates needed
      (spec template is principle-agnostic)
    - .specify/templates/tasks-template.md — ✅ no updates needed
      (foundational phase already covers DB setup tasks)
  Follow-up TODOs: none
-->

# RentalForge Constitution

## Core Principles

### I. Spec-Driven Development

Every feature MUST begin with an approved specification before any
implementation work starts. Specifications define user stories,
acceptance criteria, and success metrics. No code is written until
the spec is reviewed and accepted.

- Features MUST follow the spec-kit workflow:
  specify → clarify → plan → tasks → implement.
- Specs MUST include prioritized user stories with measurable
  acceptance scenarios.
- Implementation that deviates from the spec MUST be reconciled
  by updating the spec first, then the code.

**Rationale**: Spec-first development prevents scope creep,
ensures shared understanding, and produces traceable requirements.

### II. Test-First (NON-NEGOTIABLE)

TDD is mandatory for all production code. The Red-Green-Refactor
cycle MUST be strictly followed: write a failing test, make it
pass with minimal code, then refactor.

- Tests MUST be written and MUST fail before implementation begins.
- Every acceptance scenario in the spec MUST map to at least one
  automated test.
- Test coverage MUST NOT decrease with any commit.
- Integration tests are REQUIRED when introducing new contracts,
  inter-module communication, or shared data models.

**Rationale**: Test-first catches defects early, documents intent,
and enforces incremental, verifiable progress.

### III. Clean Architecture

The codebase MUST maintain clear separation of concerns through
layered architecture with explicit dependency direction.

- Dependencies MUST point inward: outer layers depend on inner
  layers, never the reverse.
- Domain/business logic MUST NOT depend on infrastructure,
  frameworks, or I/O concerns.
- Interfaces MUST be used at layer boundaries to enforce the
  dependency inversion principle.
- Each layer MUST be independently testable.

**Rationale**: Clean architecture keeps the domain portable,
testable, and resilient to infrastructure changes.

### IV. YAGNI and Simplicity

Code MUST solve the current requirement and nothing more. Premature
abstraction, speculative features, and over-engineering are defects.

- Do NOT add code for hypothetical future requirements.
- Prefer three similar lines over a premature abstraction.
- Every abstraction, pattern, or indirection MUST be justified by
  a concrete, present-day need documented in the spec or plan.
- Complexity additions MUST be tracked in the plan's Complexity
  Tracking table with rejected simpler alternatives.

**Rationale**: Simplicity reduces cognitive load, accelerates
delivery, and minimizes maintenance burden.

### V. Observability and Maintainability

All production code MUST be written for human comprehension and
operational transparency.

- Structured logging MUST be used for all significant operations
  (not ad-hoc Console.WriteLine).
- Error messages MUST be actionable: describe what failed, why,
  and what the user or developer can do about it.
- Public APIs and non-obvious internal logic MUST have XML
  documentation comments.
- Code MUST follow consistent naming conventions aligned with
  .NET/C# community standards.

**Rationale**: Code is read far more than it is written.
Observable systems reduce debugging time and operational risk.

### VI. Functional Style and Immutability

Code MUST favor a functional style with immutable data structures.
Side-effects MUST be minimized, isolated, and clearly signaled.

- Data structures MUST be immutable by default. Use `record`,
  `readonly record struct`, `ImmutableList<T>`,
  `ImmutableDictionary<TKey, TValue>`, and `init`-only
  properties. Mutable state is permitted only when justified by
  a concrete performance or framework constraint.
- Pure functions MUST be the default unit of logic. A pure
  function depends only on its inputs and produces only its
  return value — no observable side-effects.
- Side-effecting operations (I/O, logging, database access,
  network calls, mutable state changes) MUST be confined to
  dedicated methods or classes whose names clearly indicate the
  side-effect (e.g., `SaveToDatabase`, `SendNotification`,
  `WriteLog`).
- Side-effecting methods MUST NOT be mixed with pure business
  logic in the same method body. Compose pure transformations
  first, then apply side-effects at the boundary.
- Prefer expressions over statements: use LINQ, pattern matching,
  and switch expressions over imperative loops and conditionals
  where readability is preserved.

**Rationale**: Functional style with confined side-effects produces
code that is easier to test, reason about, and compose. Immutable
data eliminates entire classes of concurrency and aliasing bugs.

## Technology Stack

### Core

- **Language**: C# (latest stable LTS version)
- **Runtime**: .NET (latest stable LTS version)
- **Platform**: Linux (WSL2 for development)
- **Build tool**: `dotnet` CLI
- **IDE/Editor**: Claude Code (primary), any editor as secondary
- **Source control**: Git + GitHub
- **Spec tooling**: GitHub spec-kit via Claude Code

### Database

- **DBMS**: PostgreSQL (existing `dvdrental` sample database)
- **ORM**: Entity Framework Core with Npgsql provider
  (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Approach**: Scaffold existing `dvdrental` tables first using
  `dotnet ef dbcontext scaffold`, then use code-first migrations
  for all subsequent schema changes.
- Scaffolded entities MUST be reviewed and adjusted to align with
  Clean Architecture (Principle III) and Functional Style
  (Principle VI) — e.g., convert generated classes to `record`
  types where appropriate, extract domain interfaces.
- Connection strings MUST NOT be committed to source control.
  Use `appsettings.Development.json` (gitignored) or environment
  variables.

### Testing

- **Unit/Integration framework**: xUnit (with FluentAssertions
  preferred)
- **Database tests**: Testcontainers for PostgreSQL
  (`Testcontainers.PostgreSql`). All integration tests that
  touch the database MUST run against a disposable Testcontainers
  instance, never against the shared development database.
- **Test isolation**: Each test class that requires a database
  MUST provision its own container or use a shared fixture with
  per-test transaction rollback.

### API Documentation

- **Specification**: Swagger / OpenAPI
- All HTTP API endpoints MUST be documented via OpenAPI metadata
  (attributes, XML comments, or endpoint filters).
- Swagger UI MUST be enabled in development and staging
  environments.
- OpenAPI spec MUST be generated at build time or on startup and
  MUST stay in sync with the actual endpoint contracts.

### Dependency Policy

All dependencies MUST be added via NuGet. Third-party packages
MUST be justified in the plan document before adoption. Prefer
standard library capabilities over external packages when the
standard library solution is adequate.

## Development Workflow

1. **Spec phase**: Use `/speckit.specify` → `/speckit.clarify` →
   `/speckit.plan` → `/speckit.tasks` to produce approved design
   artifacts before any implementation.
2. **Branch strategy**: One feature branch per spec
   (`###-feature-name`). All work happens on feature branches.
3. **Commit discipline**: Commit after each completed task or
   logical group. Commit messages MUST describe the "why".
4. **Implementation order**: Follow tasks.md phases sequentially.
   Within a phase, tasks marked `[P]` MAY run in parallel.
5. **Review gates**: Each phase checkpoint MUST pass before
   advancing. User story checkpoints MUST demonstrate independent
   functionality.
6. **No force pushes** to main. Feature branches merge via PR.

## Governance

This constitution is the highest-authority document for the
RentalForge project. All development practices, code reviews,
and architectural decisions MUST comply with these principles.

- **Amendments**: Any change to this constitution MUST be
  documented with rationale, approved by the project owner,
  and accompanied by a migration plan if existing code is
  affected.
- **Versioning**: The constitution follows semantic versioning.
  MAJOR for principle removals or redefinitions, MINOR for new
  principles or material expansions, PATCH for clarifications
  and wording fixes.
- **Compliance**: Every PR and code review MUST verify alignment
  with these principles. Non-compliance MUST be flagged and
  resolved before merge.
- **Conflict resolution**: If a principle conflicts with another,
  the higher-numbered principle yields to the lower-numbered one
  (Spec-Driven > Test-First > Clean Architecture > YAGNI >
  Observability > Functional Style).
- **Guidance file**: See `CLAUDE.md` for runtime development
  guidance and build commands.

**Version**: 1.2.0 | **Ratified**: 2026-02-21 | **Last Amended**: 2026-02-21
