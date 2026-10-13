using Core.Application.Responses;

namespace Application.Features.Venues.Commands.Create;

public record CreatedVenueResponse(Guid Id, string Name) : IResponse;
