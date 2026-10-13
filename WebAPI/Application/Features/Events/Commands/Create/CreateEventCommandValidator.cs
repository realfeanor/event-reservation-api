using Domain.Entities;
using FluentValidation;

namespace Application.Features.Events.Commands.Create;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MaximumLength(Event.MaximumTitleLength);
        RuleFor(command => command.Description).MaximumLength(Event.MaximumDescriptionLength);
    }
}
