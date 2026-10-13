using FluentValidation;

namespace Application.Features.Reservations.Commands.HoldSeats;

public class HoldSeatsCommandValidator : AbstractValidator<HoldSeatsCommand>
{
    public HoldSeatsCommandValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.SessionId).NotEmpty();
        RuleFor(command => command.SessionSeatIds).NotEmpty();
        RuleForEach(command => command.SessionSeatIds).NotEmpty();
        RuleFor(command => command.SessionSeatIds)
            .Must(seatIds => seatIds.Distinct().Count() == seatIds.Count)
            .WithMessage("A seat can only be selected once.");
    }
}
