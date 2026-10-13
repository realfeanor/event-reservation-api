using Core.Application.Responses;
using Domain.Enums;

namespace Application.Features.Events.Commands.Create;

public record CreatedEventResponse(Guid Id, string Title, string Description, EventStatus Status) : IResponse;
