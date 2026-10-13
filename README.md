# Event Reservation API

An educational backend project for managing events, venue sessions, temporary
seat holds, and confirmed reservations.

The project is being developed as a backend-focused portfolio project. Its main
goal is to demonstrate domain-driven design, Clean Architecture, CQRS, reliable
business workflows, concurrency control, and automated testing in .NET 8.

> **Project status:** The complete first reservation workflow and portfolio
> hardening milestone are implemented.
> Customers can hold, confirm, list, and cancel reservations; a hosted worker
> expires abandoned holds. Transitions are idempotent and concurrency-controlled,
> with relational integration tests covering atomic writes and retries.

## Why This Project Exists

This is not intended to be another CRUD demonstration or a continuation of an
existing Docker/AWS project. It focuses on backend design problems such as:

- preventing two users from reserving the same seat;
- expiring temporary seat holds;
- enforcing reservation state transitions;
- making retried commands safe;
- keeping domain rules independent from HTTP and persistence concerns;
- testing business behavior at domain, application, and API levels.

## Planned First Workflow

```text
Administrator creates a venue and its seats
                    |
Administrator creates an event and scheduled session
                    |
User places a temporary hold on available seats
                    |
User confirms the reservation before the hold expires
                    |
Unconfirmed holds are released automatically
```

Payments, microservices, message brokers, and a full frontend are intentionally
outside the first milestone. They will only be considered when they support a
real requirement.

## Architecture

The solution currently contains the following layers:

- **Domain**: business concepts, state, and invariants;
- **Application**: commands, queries, validation, authorization, and use cases;
- **Persistence**: Entity Framework Core mappings and repositories;
- **Infrastructure**: integrations with external services;
- **WebAPI**: HTTP endpoints, authentication, and application composition;
- **CorePackages**: reusable architectural building blocks.

The intended request flow is:

```text
HTTP request
    -> Command or query
    -> Application handler
    -> Domain behavior
    -> Persistence
    -> Response
```

More detailed design decisions are recorded in the
[Architecture and Data Model](docs/architecture.md),
[Domain Model](docs/domain-model.md), and
[Development Roadmap](docs/roadmap.md).

## Quality Strategy

- Domain unit tests for invariants and state transitions
- Application tests for command and query behavior
- Integration tests against a relational database
- HTTP tests through `WebApplicationFactory`
- Concurrency tests for competing seat-hold requests
- Architecture tests for layer dependency rules

## Current Foundation

The inherited foundation already provides several reusable capabilities,
including MediatR pipelines, validation, authorization, caching, logging,
localization, JWT authentication, refresh tokens, and role/claim management.

The first baseline-hardening pass now uses source project references instead of
local `bin/Debug` DLLs, resolves the known dependency vulnerabilities and version
conflicts, provides safe sample configuration, and introduces executable unit,
integration, and architecture test projects.

The original feature slices now include an encapsulated `Venue` aggregate,
normalized seat-label value objects, duplicate active-seat protection, seat
deactivation, administrator-only commands, Entity Framework mappings, and a
public venue-layout query. It also includes the `Event` publication lifecycle,
UTC-only session periods, session scheduling, and stable seat-layout snapshots
that are not changed by later venue edits. Authenticated customers can create a
configurable, expiring hold for multiple seats. The pending reservation and all
seat state changes are saved together using concurrency tokens. Reservations
can then be confirmed, listed, or cancelled, while a background worker safely
expires abandoned holds and releases their seats.

## Reservation API

All reservation mutation endpoints require a bearer token. A customer identity
is always taken from that token rather than accepted from the request body.

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/Sessions/{sessionId}/seats` | Read current seat availability. |
| `POST` | `/api/Sessions/{sessionId}/seat-holds` | Hold one or more seats temporarily. |
| `GET` | `/api/Reservations` | List the authenticated customer's reservations. |
| `PUT` | `/api/Reservations/{reservationId}/confirmation` | Confirm a valid hold idempotently. |
| `DELETE` | `/api/Reservations/{reservationId}` | Cancel before the session starts. |

Example hold request:

```json
{
  "sessionSeatIds": [
    "11111111-1111-1111-1111-111111111111",
    "22222222-2222-2222-2222-222222222222"
  ]
}
```

## Local Verification

From the repository root, create local development configuration from the safe
template and replace its placeholder secrets before running the API:

```powershell
Copy-Item WebAPI/WebAPI/appsettings.example.json WebAPI/WebAPI/appsettings.Development.json
dotnet tool restore
dotnet ef database update --project WebAPI/Persistence/Persistence.csproj --startup-project WebAPI/WebAPI/WebAPI.csproj
dotnet test WebAPI/WebAPI.slnx
dotnet run --project WebAPI/WebAPI/WebAPI.csproj
```

The local configuration created by this command is ignored by Git. Runtime
persistence uses SQL Server through `ConnectionStrings:BaseDb`; integration
tests replace it with a uniquely named in-memory SQLite database so transactions
and optimistic-concurrency behavior are exercised by a relational provider.

### Dependency License Configuration

AutoMapper 15 and MediatR 14 share the registered Lucky Penny Community License.
Set its key in the `LUCKYPENNY_LICENSE_KEY` environment variable on the local
machine or hosting platform before starting the API. Do not place the key in an
`appsettings` file, launch profile, source file, or command committed to Git.

For GitHub Actions, create a repository Actions secret named
`LUCKYPENNY_LICENSE_KEY`. The CI workflow passes that secret to the build without
printing or storing it. Tests can still run without the secret, but the packages
will emit a missing-license warning.

## Operations

| Endpoint or header | Purpose |
| --- | --- |
| `GET /health/live` | Process liveness; it does not check external dependencies. |
| `GET /health/ready` | Readiness, including SQL Server connectivity. |
| `X-Correlation-ID` | Optional request identifier, returned in the response and error details. |

Unhandled failures use `application/problem+json` without exposing stack traces.
The same correlation identifier is attached to structured request logs and the
error response so a failed request can be traced safely. GitHub Actions verifies
restore, build, formatting, the Entity Framework model, and all test projects.

## Attribution and Original Work

The architectural foundation and `CorePackages` are based on the public
**nArchitecture** course material and repositories created by **Engin Demiroğ**.
The source is included in this repository so its behavior can be studied,
debugged, and reviewed alongside the application. The exact course reference,
official upstream repositories, licenses, and inherited/original boundary are
recorded in [Third-Party Notices](THIRD-PARTY-NOTICES.md).

The following areas are original portfolio work:

- event reservation domain model and terminology;
- venue, event, session, seat-hold, and reservation workflows;
- business rules and concurrency handling;
- application commands, queries, and API design;
- database model and migrations for the reservation domain;
- automated tests and architecture verification;
- technical decisions and project documentation.

Commit history and this section will be maintained so inherited foundation code
and original contributions remain easy to distinguish.

AutoMapper 15 and MediatR 14 use Lucky Penny Software licensing. This project
uses a registered Community License supplied through an untracked environment
variable; the key is intentionally absent from the repository.

## Technology

- .NET 8 and ASP.NET Core Web API
- Entity Framework Core
- MediatR and CQRS
- FluentValidation
- JWT authentication and claim-based authorization
- SQL Server

Additional technology will be introduced only when required by an implemented
use case.
