using Application.Features.Events.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exception.Types;
using Domain.Entities;
using MediatR;

namespace Application.Features.Events.Commands.Publish;

public class PublishEventCommand : IRequest<PublishedEventResponse>, ISecuredRequest
{
    public Guid EventId { get; set; }
    public string[] Roles => [EventsOperationClaims.Admin, EventsOperationClaims.Write, EventsOperationClaims.Update];

    public class Handler : IRequestHandler<PublishEventCommand, PublishedEventResponse>
    {
        private readonly IEventRepository _eventRepository;
        public Handler(IEventRepository eventRepository) => _eventRepository = eventRepository;

        public async Task<PublishedEventResponse> Handle(PublishEventCommand request, CancellationToken cancellationToken)
        {
            Event domainEvent = await _eventRepository.GetAsync(
                domainEvent => domainEvent.Id == request.EventId,
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Event not found.");
            domainEvent.Publish();
            await _eventRepository.UpdateAsync(domainEvent, cancellationToken);
            return new PublishedEventResponse(domainEvent.Id, domainEvent.Status);
        }
    }
}
