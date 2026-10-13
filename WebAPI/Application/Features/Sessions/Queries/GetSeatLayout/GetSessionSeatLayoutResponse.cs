using Core.Application.Responses;
using Domain.Enums;

namespace Application.Features.Sessions.Queries.GetSeatLayout;

public record GetSessionSeatLayoutResponse(
    Guid Id,
    Guid EventId,
    Guid VenueId,
    DateTime StartUtc,
    DateTime EndUtc,
    bool ReservationsOpen,
    IReadOnlyCollection<SessionSeatResponse> Seats
) : IResponse;

public record SessionSeatResponse(
    Guid Id,
    Guid VenueSeatId,
    string Label,
    SessionSeatStatus Status,
    DateTime? HoldExpiresAtUtc
);
