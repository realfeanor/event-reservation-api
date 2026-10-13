namespace Domain.Events;

public record SeatsHeldDomainEvent(
    Guid ReservationId,
    Guid SessionId,
    IReadOnlyCollection<Guid> SessionSeatIds,
    DateTime ExpiresAtUtc,
    DateTime OccurredAtUtc
) : IDomainEvent;

public record ReservationConfirmedDomainEvent(Guid ReservationId, DateTime OccurredAtUtc) : IDomainEvent;

public record ReservationCancelledDomainEvent(Guid ReservationId, DateTime OccurredAtUtc) : IDomainEvent;

public record ReservationExpiredDomainEvent(Guid ReservationId, DateTime OccurredAtUtc) : IDomainEvent;
