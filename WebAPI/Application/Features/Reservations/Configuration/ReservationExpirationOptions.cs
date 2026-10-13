namespace Application.Features.Reservations.Configuration;

public class ReservationExpirationOptions
{
    public const string SectionName = "ReservationExpiration";
    public int PollingIntervalSeconds { get; set; } = 60;
    public int BatchSize { get; set; } = 100;
}
