using Core.Application.Responses;
using Domain.Enums;

namespace Application.Features.Events.Commands.Publish;

public record PublishedEventResponse(Guid Id, EventStatus Status) : IResponse;
