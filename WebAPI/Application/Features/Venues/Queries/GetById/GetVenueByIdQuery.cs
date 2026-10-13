using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exception.Types;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Venues.Queries.GetById;

public class GetVenueByIdQuery : IRequest<GetVenueByIdResponse>
{
    public Guid Id { get; set; }

    public class Handler : IRequestHandler<GetVenueByIdQuery, GetVenueByIdResponse>
    {
        private readonly IVenueRepository _venueRepository;

        public Handler(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<GetVenueByIdResponse> Handle(GetVenueByIdQuery request, CancellationToken cancellationToken)
        {
            Venue venue = await _venueRepository.GetAsync(
                venue => venue.Id == request.Id,
                include: query => query.Include(venue => venue.Seats),
                enableTracking: false,
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Venue not found.");

            IReadOnlyCollection<VenueSeatResponse> seats = venue.Seats
                .OrderBy(seat => seat.Label.NormalizedValue)
                .Select(seat => new VenueSeatResponse(seat.Id, seat.Label.Value, seat.IsActive))
                .ToArray();

            return new GetVenueByIdResponse(venue.Id, venue.Name, seats);
        }
    }
}
