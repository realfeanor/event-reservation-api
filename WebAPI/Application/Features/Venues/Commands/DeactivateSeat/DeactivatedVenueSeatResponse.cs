using Core.Application.Responses;

namespace Application.Features.Venues.Commands.DeactivateSeat;

public record DeactivatedVenueSeatResponse(Guid Id, string Label, bool IsActive) : IResponse;
