using Domain.Entities;
using FluentValidation;

namespace Application.Features.Venues.Commands.Create;

public class CreateVenueCommandValidator : AbstractValidator<CreateVenueCommand>
{
    public CreateVenueCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(Venue.MaximumNameLength);
    }
}
