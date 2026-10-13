using Core.Application.Responses;
using Domain.Enums;

namespace Application.Features.Reservations.Commands;

public record ReservationStateResponse(
    Guid Id,
    ReservationStatus Status,
    DateTime? ConfirmedAtUtc,
    DateTime? CancelledAtUtc,
    DateTime? ExpiredAtUtc
) : IResponse;
