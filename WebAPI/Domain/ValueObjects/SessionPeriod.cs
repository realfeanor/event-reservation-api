using Domain.Exceptions;

namespace Domain.ValueObjects;

public sealed class SessionPeriod : IEquatable<SessionPeriod>
{
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }

    private SessionPeriod() { }

    private SessionPeriod(DateTime startUtc, DateTime endUtc)
    {
        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    public static SessionPeriod Create(DateTime startUtc, DateTime endUtc)
    {
        if (startUtc.Kind != DateTimeKind.Utc || endUtc.Kind != DateTimeKind.Utc)
            throw new DomainRuleException("Session start and end times must be UTC.");

        if (endUtc <= startUtc)
            throw new DomainRuleException("Session end time must be later than its start time.");

        return new SessionPeriod(startUtc, endUtc);
    }

    public bool HasStarted(DateTime utcNow)
    {
        EnsureUtc(utcNow);
        return utcNow >= StartUtc;
    }

    public bool Equals(SessionPeriod? other) =>
        other is not null && StartUtc == other.StartUtc && EndUtc == other.EndUtc;

    public override bool Equals(object? obj) => obj is SessionPeriod other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(StartUtc, EndUtc);

    private static void EnsureUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new DomainRuleException("Time comparisons must use UTC.");
    }
}
