using Core.Persistence.Repositories;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Session : Entity<Guid>
{
    private readonly List<SessionSeat> _seats = [];

    public Guid EventId { get; private set; }
    public Guid VenueId { get; private set; }
    public SessionPeriod Period { get; private set; }
    public bool ReservationsOpen { get; private set; }
    public IReadOnlyCollection<SessionSeat> Seats => _seats;

    private Session()
    {
        Period = null!;
    }

    private Session(Guid id, Guid eventId, Guid venueId, SessionPeriod period, IEnumerable<Seat> venueSeats)
        : base(id)
    {
        if (eventId == Guid.Empty)
            throw new DomainRuleException("A session must reference an event.");
        if (venueId == Guid.Empty)
            throw new DomainRuleException("A session must reference a venue.");

        EventId = eventId;
        VenueId = venueId;
        Period = period;

        Seat[] activeSeats = venueSeats.Where(seat => seat.IsActive).ToArray();
        if (activeSeats.Length == 0)
            throw new DomainRuleException("A session requires at least one active venue seat.");

        _seats.AddRange(activeSeats.Select(seat => new SessionSeat(Guid.NewGuid(), id, seat)));
    }

    public static Session Schedule(
        Guid eventId,
        Guid venueId,
        DateTime startUtc,
        DateTime endUtc,
        IEnumerable<Seat> venueSeats
    ) => new(Guid.NewGuid(), eventId, venueId, SessionPeriod.Create(startUtc, endUtc), venueSeats);

    public void OpenReservations(DateTime utcNow)
    {
        if (Period.HasStarted(utcNow))
            throw new DomainRuleException("Reservations cannot be opened after the session has started.");

        ReservationsOpen = true;
    }

    public void CloseReservations() => ReservationsOpen = false;

    public bool CanAcceptReservations(DateTime utcNow) => ReservationsOpen && !Period.HasStarted(utcNow);

    public Reservation HoldSeats(
        Guid customerId,
        IEnumerable<Guid> sessionSeatIds,
        DateTime utcNow,
        DateTime expiresAtUtc
    )
    {
        if (!CanAcceptReservations(utcNow))
            throw new DomainRuleException("This session is not accepting reservations.");

        Guid[] requestedSeatIds = sessionSeatIds.Distinct().ToArray();
        if (requestedSeatIds.Length == 0)
            throw new DomainRuleException("At least one seat must be selected.");

        SessionSeat[] requestedSeats = _seats.Where(seat => requestedSeatIds.Contains(seat.Id)).ToArray();
        if (requestedSeats.Length != requestedSeatIds.Length)
            throw new DomainRuleException("One or more selected seats do not belong to this session.");
        SessionSeat? unavailableSeat = requestedSeats.FirstOrDefault(seat => !seat.IsAvailable(utcNow));
        if (unavailableSeat is not null)
            throw new SeatUnavailableException(unavailableSeat.Label);

        Reservation reservation = Reservation.Create(customerId, Id, requestedSeatIds, utcNow, expiresAtUtc);
        foreach (SessionSeat seat in requestedSeats)
            seat.Hold(reservation.Id, utcNow, expiresAtUtc);

        return reservation;
    }
}
