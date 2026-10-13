using System.Text.Json.Serialization;
using Application.Features.Reservations.Configuration;
using Application.Services.Repositories;
using Application.Services.Time;
using Application.Services.DomainEvents;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exception.Types;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.Reservations.Commands.HoldSeats;

public class HoldSeatsCommand : IRequest<HeldSeatsResponse>, ISecuredRequest
{
    [JsonIgnore]
    public Guid CustomerId { get; set; }
    public Guid SessionId { get; set; }
    public IReadOnlyCollection<Guid> SessionSeatIds { get; set; } = [];
    public string[] Roles => [];

    public class Handler : IRequestHandler<HoldSeatsCommand, HeldSeatsResponse>
    {
        private const string SeatConflictMessage =
            "One or more selected seats are no longer available. Refresh the seat layout and try again.";

        private readonly ISessionRepository _sessionRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IClock _clock;
        private readonly SeatHoldOptions _options;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public Handler(
            ISessionRepository sessionRepository,
            IReservationRepository reservationRepository,
            IClock clock,
            IOptions<SeatHoldOptions> options,
            IDomainEventDispatcher domainEventDispatcher
        )
        {
            _sessionRepository = sessionRepository;
            _reservationRepository = reservationRepository;
            _clock = clock;
            _options = options.Value;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<HeldSeatsResponse> Handle(HoldSeatsCommand request, CancellationToken cancellationToken)
        {
            Session session = await _sessionRepository.GetAsync(
                session => session.Id == request.SessionId,
                include: query => query.Include(session => session.Seats),
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Session not found.");

            DateTime utcNow = _clock.UtcNow.UtcDateTime;
            DateTime expiresAtUtc = utcNow.AddMinutes(_options.DurationMinutes);
            Reservation reservation;
            try
            {
                reservation = session.HoldSeats(
                    request.CustomerId,
                    request.SessionSeatIds,
                    utcNow,
                    expiresAtUtc
                );
            }
            catch (SeatUnavailableException exception)
            {
                throw new ConflictException(SeatConflictMessage, exception);
            }

            try
            {
                await _reservationRepository.AddWithSeatsAsync(reservation, cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                throw new ConflictException(SeatConflictMessage, exception);
            }
            await _domainEventDispatcher.DispatchAndClearAsync(reservation, cancellationToken);

            return new HeldSeatsResponse(
                reservation.Id,
                reservation.SessionId,
                reservation.Status,
                reservation.ExpiresAtUtc,
                reservation.Seats.Select(seat => seat.SessionSeatId).ToArray()
            );
        }
    }
}
