using Application.Features.Venues.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exception.Types;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Venues.Commands.AddSeat;

public class AddSeatToVenueCommand : IRequest<AddedSeatToVenueResponse>, ISecuredRequest
{
    public Guid VenueId { get; set; }
    public string Label { get; set; } = string.Empty;

    public string[] Roles => [VenuesOperationClaims.Admin, VenuesOperationClaims.Write, VenuesOperationClaims.Update];

    public class Handler : IRequestHandler<AddSeatToVenueCommand, AddedSeatToVenueResponse>
    {
        private readonly IVenueRepository _venueRepository;

        public Handler(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<AddedSeatToVenueResponse> Handle(
            AddSeatToVenueCommand request,
            CancellationToken cancellationToken
        )
        {
            Venue venue = await _venueRepository.GetAsync(
                venue => venue.Id == request.VenueId,
                include: query => query.Include(venue => venue.Seats),
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Venue not found.");

            Seat seat = venue.AddSeat(request.Label);
            await _venueRepository.SaveChangesAsync(cancellationToken);
            return new AddedSeatToVenueResponse(seat.Id, seat.Label.Value, seat.IsActive);
        }
    }
}
