using Core.Application.Responses;
using Domain.Enums;

namespace Application.Features.Reservations.Commands.HoldSeats;

public record HeldSeatsResponse(
    Guid Id,
    Guid SessionId,
    ReservationStatus Status,
    DateTime ExpiresAtUtc,
    IReadOnlyCollection<Guid> SessionSeatIds
) : IResponse;
