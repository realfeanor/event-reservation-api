using Application.Services.DomainEvents;
using Core.CrossCuttingConcerns.Logging.Abstraction;
using Domain.Events;

namespace Application.Features.Reservations.EventHandlers;

public sealed class ReservationLifecycleLoggingHandler :
    IDomainEventHandler<SeatsHeldDomainEvent>,
    IDomainEventHandler<ReservationConfirmedDomainEvent>,
    IDomainEventHandler<ReservationCancelledDomainEvent>,
    IDomainEventHandler<ReservationExpiredDomainEvent>
{
    private readonly ILogger _logger;

    public ReservationLifecycleLoggingHandler(ILogger logger) => _logger = logger;

    public Task Handle(SeatsHeldDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.Information($"Reservation {domainEvent.ReservationId} placed a hold on {domainEvent.SessionSeatIds.Count} seats.");
        return Task.CompletedTask;
    }

    public Task Handle(ReservationConfirmedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.Information($"Reservation {domainEvent.ReservationId} was confirmed.");
        return Task.CompletedTask;
    }

    public Task Handle(ReservationCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.Information($"Reservation {domainEvent.ReservationId} was cancelled.");
        return Task.CompletedTask;
    }

    public Task Handle(ReservationExpiredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.Information($"Reservation {domainEvent.ReservationId} expired.");
        return Task.CompletedTask;
    }
}
