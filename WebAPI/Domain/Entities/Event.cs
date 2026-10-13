using Core.Persistence.Repositories;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class Event : Entity<Guid>
{
    public const int MaximumTitleLength = 200;
    public const int MaximumDescriptionLength = 2000;

    public string Title { get; private set; }
    public string Description { get; private set; }
    public EventStatus Status { get; private set; }

    private Event()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    private Event(Guid id, string title, string description)
        : base(id)
    {
        Title = NormalizeRequired(title, "An event title is required.", MaximumTitleLength, "event title");
        Description = NormalizeDescription(description);
        Status = EventStatus.Draft;
    }

    public static Event Create(string title, string description) => new(Guid.NewGuid(), title, description);

    public void UpdateDetails(string title, string description)
    {
        if (Status != EventStatus.Draft)
            throw new DomainRuleException("Only a draft event can be edited.");

        Title = NormalizeRequired(title, "An event title is required.", MaximumTitleLength, "event title");
        Description = NormalizeDescription(description);
    }

    public void Publish()
    {
        if (Status != EventStatus.Draft)
            throw new DomainRuleException("Only a draft event can be published.");

        Status = EventStatus.Published;
    }

    public void Cancel()
    {
        if (Status == EventStatus.Cancelled)
            return;

        Status = EventStatus.Cancelled;
    }

    private static string NormalizeDescription(string description)
    {
        string normalized = description?.Trim() ?? string.Empty;
        if (normalized.Length > MaximumDescriptionLength)
            throw new DomainRuleException($"An event description cannot exceed {MaximumDescriptionLength} characters.");

        return normalized;
    }

    private static string NormalizeRequired(string value, string requiredMessage, int maximumLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainRuleException(requiredMessage);

        string normalized = value.Trim();
        if (normalized.Length > maximumLength)
            throw new DomainRuleException($"An {fieldName} cannot exceed {maximumLength} characters.");

        return normalized;
    }
}
