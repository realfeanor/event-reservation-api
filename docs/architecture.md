# Architecture and Data Model

The application uses Clean Architecture boundaries: dependencies point toward
the domain, while the Web API composes the application and infrastructure at
runtime.

```mermaid
flowchart LR
    Client[API client] --> API[WebAPI]
    API -->|commands and queries| Application
    Application --> Domain
    Application -->|repository abstractions| Persistence
    Persistence --> Domain
    Persistence --> Database[(SQL Server)]
    Infrastructure --> Application
    API --> Infrastructure
    CorePackages[CorePackages] -. shared behaviors .-> API
    CorePackages -. shared behaviors .-> Application
    CorePackages -. persistence primitives .-> Persistence

    subgraph WebAPI_concerns [WebAPI concerns]
        API
        Correlation[Correlation and request logging]
        Errors[Problem Details errors]
        Auth[JWT authentication]
        Worker[Reservation expiration worker]
        API --- Correlation
        API --- Errors
        API --- Auth
        API --- Worker
    end
```

The HTTP layer never accepts the customer identity for a reservation from the
request body. It takes the identity from the authenticated token, sends a
command or query through MediatR, and lets the domain enforce state changes.

## Reservation Data Model

This diagram focuses on the original event-reservation tables. Authentication
support tables are intentionally omitted except for `Users`.

```mermaid
erDiagram
    USERS ||--o{ RESERVATIONS : owns
    VENUES ||--|{ SEATS : defines
    EVENTS ||--o{ SESSIONS : schedules
    VENUES ||--o{ SESSIONS : hosts
    SESSIONS ||--|{ SESSION_SEATS : snapshots
    SESSIONS ||--o{ RESERVATIONS : receives
    RESERVATIONS ||--|{ RESERVATION_SEATS : contains
    SESSION_SEATS ||--o{ RESERVATION_SEATS : selected_as

    USERS {
        uuid Id PK
        string Email
    }
    VENUES {
        uuid Id PK
        string Name
    }
    SEATS {
        uuid Id PK
        uuid VenueId FK
        string Label
        bool IsActive
    }
    EVENTS {
        uuid Id PK
        string Title
        int Status
    }
    SESSIONS {
        uuid Id PK
        uuid EventId FK
        uuid VenueId FK
        datetime StartsAtUtc
        datetime EndsAtUtc
    }
    SESSION_SEATS {
        uuid Id PK
        uuid SessionId FK
        uuid VenueSeatId
        string Label
        int Status
        uuid ReservationId
        datetime HoldExpiresAtUtc
        uuid ConcurrencyToken
    }
    RESERVATIONS {
        uuid Id PK
        uuid CustomerId FK
        uuid SessionId FK
        int Status
        datetime ExpiresAtUtc
        uuid ConcurrencyToken
    }
    RESERVATION_SEATS {
        uuid Id PK
        uuid ReservationId FK
        uuid SessionSeatId FK
    }
```

`SessionSeat` is a snapshot of the venue layout when the session is created, so
later venue edits cannot rewrite an already scheduled event. Concurrency tokens
on mutable seat and reservation state make competing holds detectable. The
reservation and all affected seats are written in one transaction, preventing
partial multi-seat holds.

## Operational Boundaries

- `X-Correlation-ID` is accepted when safe, otherwise generated, returned to the
  caller, added to the logging scope, and included as `traceId` in error bodies.
- `/health/live` reports whether the process is running.
- `/health/ready` also verifies database connectivity.
- The expiration hosted service uses the same domain transitions as foreground
  requests; repeated expiration and confirmation operations are idempotent.
