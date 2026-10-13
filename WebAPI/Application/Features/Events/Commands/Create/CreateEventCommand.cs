using Application.Features.Events.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines.Authorization;
using Domain.Entities;
using MediatR;

namespace Application.Features.Events.Commands.Create;

public class CreateEventCommand : IRequest<CreatedEventResponse>, ISecuredRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Roles => [EventsOperationClaims.Admin, EventsOperationClaims.Write, EventsOperationClaims.Create];

    public class Handler : IRequestHandler<CreateEventCommand, CreatedEventResponse>
    {
        private readonly IEventRepository _eventRepository;
        public Handler(IEventRepository eventRepository) => _eventRepository = eventRepository;

        public async Task<CreatedEventResponse> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            Event domainEvent = Event.Create(request.Title, request.Description);
            await _eventRepository.AddAsync(domainEvent, cancellationToken);
            return new CreatedEventResponse(domainEvent.Id, domainEvent.Title, domainEvent.Description, domainEvent.Status);
        }
    }
}
