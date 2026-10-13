using Core.Application.Responses;

namespace Application.Features.Venues.Commands.AddSeat;

public record AddedSeatToVenueResponse(Guid Id, string Label, bool IsActive) : IResponse;
