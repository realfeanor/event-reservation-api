using Core.Persistence.Repositories;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Venue : Entity<Guid>
{
    public const int MaximumNameLength = 200;

    private readonly List<Seat> _seats = [];

    public string Name { get; private set; }
    public IReadOnlyCollection<Seat> Seats => _seats;

    private Venue()
    {
        Name = string.Empty;
    }

    private Venue(Guid id, string name)
        : base(id)
    {
        Name = NormalizeName(name);
    }

    public static Venue Create(string name) => new(Guid.NewGuid(), name);

    public void Rename(string name) => Name = NormalizeName(name);

    public Seat AddSeat(string label)
    {
        SeatLabel seatLabel = SeatLabel.Create(label);
        bool duplicateActiveLabel = _seats.Any(seat => seat.IsActive && seat.Label == seatLabel);

        if (duplicateActiveLabel)
            throw new DomainRuleException($"An active seat with label '{seatLabel}' already exists in this venue.");

        Seat seat = new(Guid.NewGuid(), Id, seatLabel);
        _seats.Add(seat);
        return seat;
    }

    public void DeactivateSeat(Guid seatId)
    {
        Seat seat = _seats.SingleOrDefault(candidate => candidate.Id == seatId)
            ?? throw new DomainRuleException("The seat does not belong to this venue.");

        seat.Deactivate();
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainRuleException("A venue name is required.");

        string normalizedName = name.Trim();
        if (normalizedName.Length > MaximumNameLength)
            throw new DomainRuleException($"A venue name cannot exceed {MaximumNameLength} characters.");

        return normalizedName;
    }
}
