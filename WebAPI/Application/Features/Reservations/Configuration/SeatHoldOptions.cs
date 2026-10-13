namespace Application.Features.Reservations.Configuration;

public class SeatHoldOptions
{
    public const string SectionName = "SeatHold";
    public int DurationMinutes { get; set; } = 10;
}
