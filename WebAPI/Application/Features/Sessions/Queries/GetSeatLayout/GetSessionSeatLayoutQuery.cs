using Application.Services.Repositories;
using Application.Services.Time;
using Core.CrossCuttingConcerns.Exception.Types;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Queries.GetSeatLayout;

public class GetSessionSeatLayoutQuery : IRequest<GetSessionSeatLayoutResponse>
{
    public Guid SessionId { get; set; }

    public class Handler : IRequestHandler<GetSessionSeatLayoutQuery, GetSessionSeatLayoutResponse>
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IClock _clock;
        public Handler(ISessionRepository sessionRepository, IClock clock)
        {
            _sessionRepository = sessionRepository;
            _clock = clock;
        }

        public async Task<GetSessionSeatLayoutResponse> Handle(
            GetSessionSeatLayoutQuery request,
            CancellationToken cancellationToken
        )
        {
            Session session = await _sessionRepository.GetAsync(
                session => session.Id == request.SessionId,
                include: query => query.Include(session => session.Seats),
                enableTracking: false,
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Session not found.");

            DateTime utcNow = _clock.UtcNow.UtcDateTime;
            SessionSeatResponse[] seats = session.Seats
                .OrderBy(seat => seat.Label)
                .Select(seat => new SessionSeatResponse(
                    seat.Id,
                    seat.VenueSeatId,
                    seat.Label,
                    seat.IsAvailable(utcNow) ? SessionSeatStatus.Available : seat.Status,
                    seat.IsAvailable(utcNow) ? null : seat.HoldExpiresAtUtc
                ))
                .ToArray();
            return new GetSessionSeatLayoutResponse(
                session.Id,
                session.EventId,
                session.VenueId,
                session.Period.StartUtc,
                session.Period.EndUtc,
                session.ReservationsOpen,
                seats
            );
        }
    }
}
