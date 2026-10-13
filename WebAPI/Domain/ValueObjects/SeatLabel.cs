using Domain.Exceptions;

namespace Domain.ValueObjects;

public sealed class SeatLabel : IEquatable<SeatLabel>
{
    public const int MaximumLength = 50;

    public string Value { get; private set; }
    public string NormalizedValue { get; private set; }

    private SeatLabel()
    {
        Value = string.Empty;
        NormalizedValue = string.Empty;
    }

    private SeatLabel(string value)
    {
        Value = value;
        NormalizedValue = value.ToUpperInvariant();
    }

    public static SeatLabel Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainRuleException("A seat label is required.");

        string normalizedWhitespace = string.Join(
            ' ',
            value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
        );

        if (normalizedWhitespace.Length > MaximumLength)
            throw new DomainRuleException($"A seat label cannot exceed {MaximumLength} characters.");

        return new SeatLabel(normalizedWhitespace);
    }

    public bool Equals(SeatLabel? other) =>
        other is not null && string.Equals(NormalizedValue, other.NormalizedValue, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is SeatLabel other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(NormalizedValue);

    public override string ToString() => Value;

    public static bool operator ==(SeatLabel? left, SeatLabel? right) => Equals(left, right);

    public static bool operator !=(SeatLabel? left, SeatLabel? right) => !Equals(left, right);
}
