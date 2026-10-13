using Application.Services.Time;

namespace Infrastructure.Adapters.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => TimeProvider.System.GetUtcNow();
}
