using Core.Application.Responses;

namespace Application.Features.Sessions.Commands.Schedule;

public record ScheduledSessionResponse(
    Guid Id,
    Guid EventId,
    Guid VenueId,
    DateTime StartUtc,
    DateTime EndUtc,
    int SeatCount
) : IResponse;
