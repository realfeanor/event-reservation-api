using Application.Services.Repositories;
using Application.Services.Time;
using Application.Services.DomainEvents;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reservations.Commands.Expire;

public record ExpireReservationCommand(Guid ReservationId) : IRequest<bool>;

public class ExpireReservationCommandHandler : IRequestHandler<ExpireReservationCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IClock _clock;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public ExpireReservationCommandHandler(
        IReservationRepository reservationRepository,
        ISessionRepository sessionRepository,
        IClock clock,
        IDomainEventDispatcher domainEventDispatcher
    )
    {
        _reservationRepository = reservationRepository;
        _sessionRepository = sessionRepository;
        _clock = clock;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<bool> Handle(ExpireReservationCommand request, CancellationToken cancellationToken)
    {
        Reservation? reservation = await _reservationRepository.GetAsync(
            reservation => reservation.Id == request.ReservationId,
            include: query => query.Include(reservation => reservation.Seats),
            cancellationToken: cancellationToken
        );
        if (reservation is null || reservation.Status != ReservationStatus.Pending)
            return false;

        Session? session = await _sessionRepository.GetAsync(
            session => session.Id == reservation.SessionId,
            include: query => query.Include(session => session.Seats),
            cancellationToken: cancellationToken
        );
        if (session is null)
            return false;

        DateTime utcNow = _clock.UtcNow.UtcDateTime;
        if (!reservation.Expire(session.Seats, utcNow))
            return false;

        try
        {
            await _reservationRepository.SaveStateAsync(reservation, utcNow, cancellationToken);
            await _domainEventDispatcher.DispatchAndClearAsync(reservation, cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}
