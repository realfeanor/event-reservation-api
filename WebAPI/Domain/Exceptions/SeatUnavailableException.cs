namespace Domain.Exceptions;

public sealed class SeatUnavailableException : Exception
{
    public SeatUnavailableException(string seatLabel)
        : base($"Seat '{seatLabel}' is not available.") { }
}
