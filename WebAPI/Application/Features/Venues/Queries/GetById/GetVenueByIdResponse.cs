using Core.Application.Responses;

namespace Application.Features.Venues.Queries.GetById;

public record GetVenueByIdResponse(Guid Id, string Name, IReadOnlyCollection<VenueSeatResponse> Seats) : IResponse;

public record VenueSeatResponse(Guid Id, string Label, bool IsActive);
