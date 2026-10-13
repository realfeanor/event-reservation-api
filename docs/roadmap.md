# Development Roadmap

This roadmap favors complete, testable workflows over a large number of partial
features. Each phase should finish with a buildable solution and focused commits.

## Phase 0 — Project Definition

Status: **complete**

- [x] Define the project goal and first workflow.
- [x] Establish the initial ubiquitous language.
- [x] Identify aggregate candidates and invariants.
- [x] Record attribution and planned original contributions.
- [x] Add the exact upstream repository and its usage/license information.
- [x] Review and accept or revise the domain terminology.

Exit condition: the README and domain model accurately describe the project
Mustafa intends to build.

## Phase 1 — Foundation Health

- [x] Capture a clean baseline build in CI-compatible commands.
- [x] Resolve the AutoMapper package security advisory.
- [x] Align conflicting ASP.NET Core, EF Core, and JWT dependency versions.
- [x] Replace local compiled-DLL references with source project references.
- [x] Verify that the dependency graph has no known vulnerable packages.
- [x] Add safe `appsettings.example.json` configuration.
- [x] Verify register, login, refresh-token, and authorization behavior.
- [x] Create unit, integration, and architecture test projects.
- [x] Add a testable clock abstraction.
- [x] Use the registered Lucky Penny Community License for AutoMapper 15 and
  MediatR 14 through an untracked environment variable.

Exit condition: a fresh checkout can be configured, built, and tested without
undocumented local state or known dependency warnings.

## Phase 2 — Event Catalog Vertical Slice

- [x] Implement `Venue` and seat behavior.
- [x] Implement `Event` and `Session` behavior.
- [x] Add Entity Framework configurations and migrations.
- [x] Add administrator commands and authorization claims.
- [x] Add session and seat queries.
- [x] Add domain, application, persistence, and HTTP tests.

Exit condition: an administrator can create a venue, seats, event, and session;
a customer can retrieve the session's seat layout.

## Phase 3 — Temporary Seat Holds

- [x] Implement `SessionSeat` availability state.
- [x] Implement pending `Reservation` creation.
- [x] Add a configurable hold duration.
- [x] Hold multiple seats atomically.
- [x] Add optimistic concurrency tokens.
- [x] Translate concurrency failures into stable API errors.
- [x] Test simultaneous requests competing for the same seat.

Exit condition: exactly one of two concurrent requests can hold the same seat,
and a failed request does not partially hold other requested seats.

## Phase 4 — Confirmation, Cancellation, and Expiration

- [x] Confirm a valid pending reservation.
- [x] Make confirmation requests idempotent.
- [x] Define and implement cancellation policy.
- [x] Expire pending reservations using a hosted background service.
- [x] Release seats atomically when holds expire or are cancelled.
- [x] Add domain events and in-process handlers.
- [x] Test retries and repeated background processing.

Exit condition: the complete first workflow operates reliably and all state
transitions are covered by automated tests.

## Phase 5 — Portfolio Hardening

- [x] Add architecture dependency tests.
- [x] Add structured logging and correlation identifiers.
- [x] Add health checks and production-safe error responses.
- [x] Document API examples and important design decisions.
- [x] Add a database diagram and architecture diagram.
- [x] Add CI for restore, build, test, and formatting checks.
- [x] Review secrets, configuration, and repository hygiene.

Exit condition: a recruiter can understand, run, and evaluate the project from
the repository without private guidance.

## Optional Phase 6 — Operational Extensions

These items are not commitments. Add them only if the completed application
creates a real reason for them.

- Transactional outbox and durable event processing
- OpenTelemetry traces and metrics
- Redis-backed distributed idempotency or caching
- Dockerfile or Docker Compose for convenient evaluation
- Image storage for event artwork
- Search indexing for a sufficiently large event catalog

AWS deployment, Kubernetes, and microservice extraction are deliberately not on
the core roadmap because they would repeat skills demonstrated elsewhere and do
not solve the first release's main domain problems.

## Working Rules

1. Implement one use case through every required layer before starting another.
2. Add tests with the behavior, not as a final cleanup phase.
3. Keep controllers thin and persistence concerns out of the domain.
4. Use inherited CorePackages only when they fit the use case.
5. Do not introduce technology solely to list it in the README.
6. Record significant architectural choices and their trade-offs.
7. Keep inherited code and original contributions clearly attributable.
