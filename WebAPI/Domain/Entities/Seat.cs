using Core.Persistence.Repositories;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Seat : Entity<Guid>
{
    public Guid VenueId { get; private set; }
    public SeatLabel Label { get; private set; }
    public bool IsActive { get; private set; }

    private Seat()
    {
        Label = null!;
    }

    internal Seat(Guid id, Guid venueId, SeatLabel label)
        : base(id)
    {
        VenueId = venueId;
        Label = label;
        IsActive = true;
    }

    internal void Deactivate() => IsActive = false;
}
