using FluentValidation;

namespace Application.Features.Sessions.Commands.Schedule;

public class ScheduleSessionCommandValidator : AbstractValidator<ScheduleSessionCommand>
{
    public ScheduleSessionCommandValidator()
    {
        RuleFor(command => command.EventId).NotEmpty();
        RuleFor(command => command.VenueId).NotEmpty();
        RuleFor(command => command.StartUtc).NotEmpty();
        RuleFor(command => command.EndUtc).NotEmpty();
    }
}
