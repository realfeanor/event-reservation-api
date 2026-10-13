using Core.Persistence.Repositories;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Events;

namespace Domain.Entities;

public class Reservation : Entity<Guid>
{
    private readonly List<ReservationSeat> _seats = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid CustomerId { get; private set; }
    public Guid SessionId { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? ExpiredAtUtc { get; private set; }
    public Guid ConcurrencyToken { get; private set; }
    public IReadOnlyCollection<ReservationSeat> Seats => _seats;
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    private Reservation() { }

    private Reservation(
        Guid id,
        Guid customerId,
        Guid sessionId,
        IEnumerable<Guid> sessionSeatIds,
        DateTime utcNow,
        DateTime expiresAtUtc
    ) : base(id)
    {
        if (customerId == Guid.Empty)
            throw new DomainRuleException("A reservation must reference a customer.");
        if (sessionId == Guid.Empty)
            throw new DomainRuleException("A reservation must reference a session.");
        if (expiresAtUtc <= utcNow)
            throw new DomainRuleException("A reservation hold must expire in the future.");

        Guid[] seatIds = sessionSeatIds.Distinct().ToArray();
        if (seatIds.Length == 0)
            throw new DomainRuleException("A reservation must contain at least one seat.");

        CustomerId = customerId;
        SessionId = sessionId;
        Status = ReservationStatus.Pending;
        ExpiresAtUtc = expiresAtUtc;
        ConcurrencyToken = Guid.NewGuid();
        _seats.AddRange(seatIds.Select(sessionSeatId => new ReservationSeat(Guid.NewGuid(), id, sessionSeatId)));
        _domainEvents.Add(new SeatsHeldDomainEvent(id, sessionId, seatIds, expiresAtUtc, utcNow));
    }

    internal static Reservation Create(
        Guid customerId,
        Guid sessionId,
        IEnumerable<Guid> sessionSeatIds,
        DateTime utcNow,
        DateTime expiresAtUtc
    ) => new(Guid.NewGuid(), customerId, sessionId, sessionSeatIds, utcNow, expiresAtUtc);

    public bool Confirm(IEnumerable<SessionSeat> sessionSeats, DateTime sessionStartUtc, DateTime utcNow)
    {
        if (Status == ReservationStatus.Confirmed)
            return false;
        if (Status != ReservationStatus.Pending)
            throw new DomainRuleException("Only a pending reservation can be confirmed.");
        if (ExpiresAtUtc <= utcNow)
            throw new DomainRuleException("An expired reservation cannot be confirmed.");
        if (utcNow >= sessionStartUtc)
            throw new DomainRuleException("A reservation cannot be confirmed after the session has started.");

        SessionSeat[] seats = GetReservedSessionSeats(sessionSeats);
        SessionSeat? invalidSeat = seats.FirstOrDefault(seat => !seat.CanBeConfirmedBy(Id, utcNow));
        if (invalidSeat is not null)
            throw new DomainRuleException($"Seat '{invalidSeat.Label}' is no longer held by this reservation.");

        foreach (SessionSeat seat in seats)
            seat.Confirm(Id, utcNow);

        Status = ReservationStatus.Confirmed;
        ConfirmedAtUtc = utcNow;
        ConcurrencyToken = Guid.NewGuid();
        _domainEvents.Add(new ReservationConfirmedDomainEvent(Id, utcNow));
        return true;
    }

    public bool Cancel(IEnumerable<SessionSeat> sessionSeats, DateTime sessionStartUtc, DateTime utcNow)
    {
        if (Status == ReservationStatus.Cancelled)
            return false;
        if (Status == ReservationStatus.Expired)
            throw new DomainRuleException("An expired reservation cannot be cancelled.");
        if (utcNow >= sessionStartUtc)
            throw new DomainRuleException("A reservation cannot be cancelled after the session has started.");

        foreach (SessionSeat seat in GetReservedSessionSeats(sessionSeats))
            seat.Release(Id);

        Status = ReservationStatus.Cancelled;
        CancelledAtUtc = utcNow;
        ConcurrencyToken = Guid.NewGuid();
        _domainEvents.Add(new ReservationCancelledDomainEvent(Id, utcNow));
        return true;
    }

    public bool Expire(IEnumerable<SessionSeat> sessionSeats, DateTime utcNow)
    {
        if (Status != ReservationStatus.Pending)
            return false;
        if (ExpiresAtUtc > utcNow)
            throw new DomainRuleException("A reservation cannot expire before its hold ends.");

        foreach (SessionSeat seat in GetReservedSessionSeats(sessionSeats))
            seat.Release(Id);

        Status = ReservationStatus.Expired;
        ExpiredAtUtc = utcNow;
        ConcurrencyToken = Guid.NewGuid();
        _domainEvents.Add(new ReservationExpiredDomainEvent(Id, utcNow));
        return true;
    }

    public IReadOnlyCollection<IDomainEvent> DequeueDomainEvents()
    {
        IDomainEvent[] events = _domainEvents.ToArray();
        _domainEvents.Clear();
        return events;
    }

    private SessionSeat[] GetReservedSessionSeats(IEnumerable<SessionSeat> sessionSeats)
    {
        Guid[] reservedSeatIds = _seats.Select(seat => seat.SessionSeatId).ToArray();
        SessionSeat[] seats = sessionSeats.Where(seat => reservedSeatIds.Contains(seat.Id)).ToArray();
        if (seats.Length != reservedSeatIds.Length)
            throw new DomainRuleException("One or more reservation seats do not belong to its session.");
        return seats;
    }
}
