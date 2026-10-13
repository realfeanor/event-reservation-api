using Application.Features.Venues.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exception.Types;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Venues.Commands.DeactivateSeat;

public class DeactivateVenueSeatCommand : IRequest<DeactivatedVenueSeatResponse>, ISecuredRequest
{
    public Guid VenueId { get; set; }
    public Guid SeatId { get; set; }

    public string[] Roles => [VenuesOperationClaims.Admin, VenuesOperationClaims.Write, VenuesOperationClaims.Update];

    public class Handler : IRequestHandler<DeactivateVenueSeatCommand, DeactivatedVenueSeatResponse>
    {
        private readonly IVenueRepository _venueRepository;

        public Handler(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<DeactivatedVenueSeatResponse> Handle(
            DeactivateVenueSeatCommand request,
            CancellationToken cancellationToken
        )
        {
            Venue venue = await _venueRepository.GetAsync(
                venue => venue.Id == request.VenueId,
                include: query => query.Include(venue => venue.Seats),
                cancellationToken: cancellationToken
            ) ?? throw new NotFoundException("Venue not found.");

            venue.DeactivateSeat(request.SeatId);
            await _venueRepository.SaveChangesAsync(cancellationToken);
            Seat seat = venue.Seats.Single(seat => seat.Id == request.SeatId);
            return new DeactivatedVenueSeatResponse(seat.Id, seat.Label.Value, seat.IsActive);
        }
    }
}
