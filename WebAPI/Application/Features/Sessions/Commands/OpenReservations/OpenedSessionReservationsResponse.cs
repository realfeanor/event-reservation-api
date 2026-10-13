using Core.Application.Responses;

namespace Application.Features.Sessions.Commands.OpenReservations;

public record OpenedSessionReservationsResponse(Guid Id, bool ReservationsOpen) : IResponse;
