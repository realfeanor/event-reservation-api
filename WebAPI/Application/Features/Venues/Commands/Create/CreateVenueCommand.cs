using Application.Features.Venues.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines.Authorization;
using Domain.Entities;
using MediatR;

namespace Application.Features.Venues.Commands.Create;

public class CreateVenueCommand : IRequest<CreatedVenueResponse>, ISecuredRequest
{
    public string Name { get; set; } = string.Empty;

    public string[] Roles => [VenuesOperationClaims.Admin, VenuesOperationClaims.Write, VenuesOperationClaims.Create];

    public class Handler : IRequestHandler<CreateVenueCommand, CreatedVenueResponse>
    {
        private readonly IVenueRepository _venueRepository;

        public Handler(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<CreatedVenueResponse> Handle(CreateVenueCommand request, CancellationToken cancellationToken)
        {
            Venue venue = Venue.Create(request.Name);
            await _venueRepository.AddAsync(venue, cancellationToken);

            return new CreatedVenueResponse(venue.Id, venue.Name);
        }
    }
}
