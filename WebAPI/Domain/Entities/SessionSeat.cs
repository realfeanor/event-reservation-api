using Core.Persistence.Repositories;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class SessionSeat : Entity<Guid>
{
    public Guid SessionId { get; private set; }
    public Guid VenueSeatId { get; private set; }
    public string Label { get; private set; }
    public SessionSeatStatus Status { get; private set; }
    public Guid? ReservationId { get; private set; }
    public DateTime? HoldExpiresAtUtc { get; private set; }
    public Guid ConcurrencyToken { get; private set; }

    private SessionSeat()
    {
        Label = string.Empty;
    }

    internal SessionSeat(Guid id, Guid sessionId, Seat venueSeat)
        : base(id)
    {
        SessionId = sessionId;
        VenueSeatId = venueSeat.Id;
        Label = venueSeat.Label.Value;
        Status = SessionSeatStatus.Available;
        ConcurrencyToken = Guid.NewGuid();
    }

    public bool IsAvailable(DateTime utcNow) =>
        Status == SessionSeatStatus.Available
        || Status == SessionSeatStatus.Held && HoldExpiresAtUtc <= utcNow;

    internal bool CanBeConfirmedBy(Guid reservationId, DateTime utcNow) =>
        Status == SessionSeatStatus.Held
        && ReservationId == reservationId
        && HoldExpiresAtUtc > utcNow;

    internal void Hold(Guid reservationId, DateTime utcNow, DateTime expiresAtUtc)
    {
        if (reservationId == Guid.Empty)
            throw new DomainRuleException("A seat hold must reference a reservation.");
        if (expiresAtUtc <= utcNow)
            throw new DomainRuleException("A seat hold must expire in the future.");
        if (!IsAvailable(utcNow))
            throw new SeatUnavailableException(Label);

        Status = SessionSeatStatus.Held;
        ReservationId = reservationId;
        HoldExpiresAtUtc = expiresAtUtc;
        ConcurrencyToken = Guid.NewGuid();
    }

    internal void Confirm(Guid reservationId, DateTime utcNow)
    {
        if (!CanBeConfirmedBy(reservationId, utcNow))
            throw new DomainRuleException($"Seat '{Label}' is not held by this reservation.");

        Status = SessionSeatStatus.Confirmed;
        HoldExpiresAtUtc = null;
        ConcurrencyToken = Guid.NewGuid();
    }

    internal void Release(Guid reservationId)
    {
        if (ReservationId != reservationId || Status == SessionSeatStatus.Available)
            return;

        Status = SessionSeatStatus.Available;
        ReservationId = null;
        HoldExpiresAtUtc = null;
        ConcurrencyToken = Guid.NewGuid();
    }
}
