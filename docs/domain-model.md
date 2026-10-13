# Domain Model

## Product Vision

The Event Reservation API allows organizers to schedule seated events and lets
customers temporarily hold and then confirm seats without double booking.

The first release is intentionally narrow. It covers reservation behavior, not
payment processing, ticket scanning, recommendations, or event discovery.

## Ubiquitous Language

| Term | Meaning |
| --- | --- |
| Venue | A physical location in which sessions take place. |
| Seat | A reservable position belonging to a venue. |
| Event | The advertised event, such as a concert or conference. |
| Session | One scheduled occurrence of an event at a venue. |
| Session seat | The reservable state of one venue seat for one session. |
| Hold | A temporary exclusive claim on one or more session seats. |
| Reservation | A customer's attempt to reserve seats for one session. |
| Hold expiration | The UTC time after which an unconfirmed hold is invalid. |
| Confirmation | The transition that makes a valid held reservation final. |
| Cancellation | The explicit termination of a reservation according to policy. |

The code and API should use these terms consistently. Synonyms such as booking,
order, performance, and screening should not be mixed into the model unless the
language is deliberately revised.

## Initial Domain Boundary

The application begins as a modular monolith with three conceptual areas:

1. **Identity and Access** — inherited user, authentication, and authorization
   capabilities.
2. **Event Catalog** — venues, seats, events, and scheduled sessions.
3. **Reservations** — seat availability, holds, confirmations, expirations, and
   cancellations.

These areas remain in the existing solution layers initially. Separate services
or deployable applications are not required.

## Aggregate Candidates

Aggregate boundaries are hypotheses and may be refined during implementation.

### Venue

Owns venue identity and descriptive information. Seats belong to a venue and
cannot be moved between venues after a session has been scheduled from the
venue's layout.

Initial behavior:

- create and rename a venue;
- add or deactivate a seat;
- prevent duplicate seat labels within a venue.

Seat labels are compared case-insensitively after repeated whitespace is
collapsed. A deactivated seat keeps its identity for historical references. A
new active seat may reuse its label, but two active seats in the same venue can
never have the same normalized label.

### Event

Owns event identity, title, description, and publication state. Scheduling is
kept outside the aggregate if the number of sessions can grow independently.

Initial behavior:

- create an event;
- update draft details;
- publish or cancel according to its state.

### Session

Represents one occurrence of an event at a venue during a time range. A session
captures which venue layout is available for reservation.

Scheduling copies the venue's active seats into session-seat records. Later
venue edits therefore affect only future sessions and cannot silently change an
existing session's reservable layout.

Initial behavior:

- schedule an event at a venue;
- prevent an invalid or empty time range;
- prevent reservation after the session begins;
- control whether reservations are open.

### SessionSeat

Protects the availability of one seat in one session. It is the concurrency
boundary that prevents two active reservations from owning the same seat.

Implemented availability state:

```text
Available -> Held -> Confirmed
               |          |
               v          v
           Available   Cancelled (policy dependent)
```

Each persisted session seat has an application-managed concurrency token. A
database conflict is translated into a stable `409 Conflict` response rather
than exposing an Entity Framework exception.

### Reservation

Owns a customer's reservation lifecycle for seats in exactly one session.

Reservation states:

```text
Pending -> Confirmed -> Cancelled
   |            |
   v            v
Expired      Cancelled
```

The reservation records its seat identifiers and hold expiration. Confirmation,
cancellation, and expiration are explicit, idempotent transitions; creation
never silently confirms a reservation.

## Candidate Value Objects

- `SeatLabel`: normalized section, row, and number or a validated display label;
- `SessionPeriod`: a valid UTC start/end pair;
- `HoldPeriod`: creation and expiration timestamps;
- `Money`: amount and currency, only when pricing enters scope;
- strongly typed identifiers, if they integrate cleanly with the inherited
  generic entity infrastructure.

Value objects should be introduced because they protect a real invariant, not
only to increase the number of DDD patterns in the project.

## Core Invariants

1. A venue cannot contain duplicate active seat labels.
2. A session must reference an existing event and venue.
3. A session end must be later than its start.
4. A reservation contains at least one seat and belongs to one session.
5. All seats in a reservation belong to the reservation's session.
6. A session seat can have at most one active hold or confirmation.
7. A hold must have a future expiration when created.
8. An expired reservation cannot be confirmed.
9. Only a pending reservation can be confirmed or expired.
10. Confirmation is idempotent: retrying the same request does not create a
    second reservation or duplicate side effects.
11. Cancellation follows an explicit policy based on reservation and session
    state.
12. Domain time comparisons use an injected clock rather than local system time.

## First Use Cases

### Administration

- Create a venue.
- Add seats to a venue.
- Create an event.
- Schedule a session.
- Open a session for reservations.

### Customer

- View available seats for a session.
- Hold one or more seats temporarily.
- Confirm a reservation.
- View the current user's reservations.
- Cancel a reservation when policy permits.

### System

- Find expired pending reservations.
- expire them safely;
- release their held session seats;
- record the outcome so repeated processing is harmless.

## Candidate Domain Events

- `SeatsHeldDomainEvent`
- `ReservationConfirmedDomainEvent`
- `ReservationExpiredDomainEvent`
- `ReservationCancelledDomainEvent`

These domain events are handled in process after a successful database commit.
Their current handler records lifecycle activity. A transactional outbox remains
deferred until event delivery must survive process failure.

## Cancellation Policy

A customer may cancel their own pending or confirmed reservation until the
session starts. Cancellation is idempotent and atomically releases seats still
owned by that reservation. Expired reservations cannot be cancelled, and an old
reservation can never release a seat that has since been held by another one.

## Concurrency Strategy

Seat allocation is the project's central consistency problem.

The implemented approach uses optimistic concurrency:

1. Load the requested `SessionSeat` records.
2. Reject seats that are confirmed or have an unexpired hold.
3. Claim all requested seats and create the pending reservation in one database
   transaction.
4. Save using concurrency tokens.
5. Convert a losing concurrent update into a domain-level seat-unavailable
   result.

The implementation must include an integration test in which two independent
requests compete for the same seat and only one succeeds.

## Time Handling

All persisted timestamps will use UTC. Domain and application code will obtain
the current time through an abstraction such as `IClock`. Tests will use a fake
clock so expiration behavior remains deterministic.

## Decisions Deferred

- payment provider and refund policy;
- pricing tiers and discounts;
- ticket generation and check-in;
- general-admission events;
- multi-tenant organizers;
- external message broker;
- full-text event search;
- frontend application.

Deferral means these concerns must not influence the first persistence model or
API unless a current use case requires them.
