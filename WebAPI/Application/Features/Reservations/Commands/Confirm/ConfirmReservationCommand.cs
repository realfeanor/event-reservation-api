using System.Text.Json.Serialization;
using Application.Features.Reservations.Commands;
using Application.Services.Repositories;
using Application.Services.Time;
using Application.Services.DomainEvents;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exception.Types;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reservations.Commands.Confirm;

public class ConfirmReservationCommand : IRequest<ReservationStateResponse>, ISecuredRequest
{
    public Guid ReservationId { get; set; }
    [JsonIgnore]
    public Guid CustomerId { get; set; }
    public string[] Roles => [];

    public class Handler : IRequestHandler<ConfirmReservationCommand, ReservationStateResponse>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public Handler(
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

        public async Task<ReservationStateResponse> Handle(
            ConfirmReservationCommand request,
            CancellationToken cancellationToken
        )
        {
            Reservation reservation = await _reservationRepository.GetAsync(
                reservation => reservation.Id == request.ReservationId && reservation.CustomerId == request.CustomerId,
                include: query => query.Include(reservation => reservation.Seats),
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Reservation not found.");

            if (reservation.Status == ReservationStatus.Confirmed)
                return ToResponse(reservation);

            Session session = await _sessionRepository.GetAsync(
                session => session.Id == reservation.SessionId,
                include: query => query.Include(session => session.Seats),
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Session not found.");

            DateTime utcNow = _clock.UtcNow.UtcDateTime;
            reservation.Confirm(session.Seats, session.Period.StartUtc, utcNow);
            try
            {
                await _reservationRepository.SaveStateAsync(reservation, utcNow, cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                ReservationStatus? currentStatus = await _reservationRepository.GetStatusAsync(
                    reservation.Id,
                    cancellationToken
                );
                if (currentStatus == ReservationStatus.Confirmed)
                    return new ReservationStateResponse(reservation.Id, ReservationStatus.Confirmed, utcNow, null, null);
                throw new ConflictException("The reservation changed while it was being confirmed. Try again.", exception);
            }
            await _domainEventDispatcher.DispatchAndClearAsync(reservation, cancellationToken);

            return ToResponse(reservation);
        }

        private static ReservationStateResponse ToResponse(Reservation reservation) =>
            new(
                reservation.Id,
                reservation.Status,
                reservation.ConfirmedAtUtc,
                reservation.CancelledAtUtc,
                reservation.ExpiredAtUtc
            );
    }
}
