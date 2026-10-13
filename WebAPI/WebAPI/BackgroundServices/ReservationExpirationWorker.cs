using Application.Features.Reservations.Commands.Expire;
using Application.Features.Reservations.Configuration;
using Application.Services.Repositories;
using Application.Services.Time;
using MediatR;
using Microsoft.Extensions.Options;

namespace WebAPI.BackgroundServices;

public sealed class ReservationExpirationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ReservationExpirationOptions _options;
    private readonly ILogger<ReservationExpirationWorker> _logger;

    public ReservationExpirationWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<ReservationExpirationOptions> options,
        ILogger<ReservationExpirationWorker> logger
    )
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(_options.PollingIntervalSeconds));
        do
        {
            try
            {
                await ExpireBatch(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while expiring reservations.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ExpireBatch(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> reservationIds;
        using (IServiceScope queryScope = _scopeFactory.CreateScope())
        {
            IReservationRepository repository = queryScope.ServiceProvider.GetRequiredService<IReservationRepository>();
            IClock clock = queryScope.ServiceProvider.GetRequiredService<IClock>();
            reservationIds = await repository.GetExpiredPendingIdsAsync(
                clock.UtcNow.UtcDateTime,
                _options.BatchSize,
                cancellationToken
            );
        }

        foreach (Guid reservationId in reservationIds)
        {
            using IServiceScope commandScope = _scopeFactory.CreateScope();
            IMediator mediator = commandScope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new ExpireReservationCommand(reservationId), cancellationToken);
        }

        if (reservationIds.Count > 0)
            _logger.LogInformation("Processed {ReservationCount} expired reservations.", reservationIds.Count);
    }
}
