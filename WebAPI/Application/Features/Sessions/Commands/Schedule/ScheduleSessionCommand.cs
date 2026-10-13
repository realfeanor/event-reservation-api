using Application.Features.Sessions.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exception.Types;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Commands.Schedule;

public class ScheduleSessionCommand : IRequest<ScheduledSessionResponse>, ISecuredRequest
{
    public Guid EventId { get; set; }
    public Guid VenueId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string[] Roles => [SessionsOperationClaims.Admin, SessionsOperationClaims.Write, SessionsOperationClaims.Create];

    public class Handler : IRequestHandler<ScheduleSessionCommand, ScheduledSessionResponse>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly ISessionRepository _sessionRepository;

        public Handler(
            IEventRepository eventRepository,
            IVenueRepository venueRepository,
            ISessionRepository sessionRepository
        )
        {
            _eventRepository = eventRepository;
            _venueRepository = venueRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task<ScheduledSessionResponse> Handle(
            ScheduleSessionCommand request,
            CancellationToken cancellationToken
        )
        {
            Event domainEvent = await _eventRepository.GetAsync(
                domainEvent => domainEvent.Id == request.EventId,
                enableTracking: false,
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Event not found.");
            if (domainEvent.Status != EventStatus.Published)
                throw new BusinessException("Only a published event can be scheduled.");

            Venue venue = await _venueRepository.GetAsync(
                venue => venue.Id == request.VenueId,
                include: query => query.Include(venue => venue.Seats),
                enableTracking: false,
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Venue not found.");

            Session session = Session.Schedule(
                domainEvent.Id,
                venue.Id,
                request.StartUtc,
                request.EndUtc,
                venue.Seats
            );
            await _sessionRepository.AddWithSeatsAsync(session, cancellationToken);
            return new ScheduledSessionResponse(
                session.Id,
                session.EventId,
                session.VenueId,
                session.Period.StartUtc,
                session.Period.EndUtc,
                session.Seats.Count
            );
        }
    }
}
