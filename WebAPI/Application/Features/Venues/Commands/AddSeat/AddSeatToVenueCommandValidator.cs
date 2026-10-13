using Domain.ValueObjects;
using FluentValidation;

namespace Application.Features.Venues.Commands.AddSeat;

public class AddSeatToVenueCommandValidator : AbstractValidator<AddSeatToVenueCommand>
{
    public AddSeatToVenueCommandValidator()
    {
        RuleFor(command => command.VenueId).NotEmpty();
        RuleFor(command => command.Label).NotEmpty().MaximumLength(SeatLabel.MaximumLength);
    }
}
