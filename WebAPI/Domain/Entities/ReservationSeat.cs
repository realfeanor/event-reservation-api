using Core.Persistence.Repositories;

namespace Domain.Entities;

public class ReservationSeat : Entity<Guid>
{
    public Guid ReservationId { get; private set; }
    public Guid SessionSeatId { get; private set; }

    private ReservationSeat() { }

    internal ReservationSeat(Guid id, Guid reservationId, Guid sessionSeatId) : base(id)
    {
        ReservationId = reservationId;
        SessionSeatId = sessionSeatId;
    }
}
