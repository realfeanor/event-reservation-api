using Application.Features.Sessions.Constants;
using Application.Services.Repositories;
using Application.Services.Time;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exception.Types;
using Domain.Entities;
using MediatR;

namespace Application.Features.Sessions.Commands.OpenReservations;

public class OpenSessionReservationsCommand : IRequest<OpenedSessionReservationsResponse>, ISecuredRequest
{
    public Guid SessionId { get; set; }
    public string[] Roles => [SessionsOperationClaims.Admin, SessionsOperationClaims.Write, SessionsOperationClaims.Update];

    public class Handler : IRequestHandler<OpenSessionReservationsCommand, OpenedSessionReservationsResponse>
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IClock _clock;
        public Handler(ISessionRepository sessionRepository, IClock clock)
        {
            _sessionRepository = sessionRepository;
            _clock = clock;
        }

        public async Task<OpenedSessionReservationsResponse> Handle(
            OpenSessionReservationsCommand request,
            CancellationToken cancellationToken
        )
        {
            Session session = await _sessionRepository.GetAsync(
                session => session.Id == request.SessionId,
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Session not found.");
            session.OpenReservations(_clock.UtcNow.UtcDateTime);
            await _sessionRepository.UpdateAsync(session, cancellationToken);
            return new OpenedSessionReservationsResponse(session.Id, session.ReservationsOpen);
        }
    }
}
