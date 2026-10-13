using Domain.Enums;
using Domain.Exceptions;
using DomainEvent = Domain.Entities.Event;
using Xunit;

namespace Domain.UnitTests;

public class EventTests
{
    [Fact]
    public void Create_ShouldCreateNormalizedDraft()
    {
        DomainEvent domainEvent = DomainEvent.Create("  Architecture Conference ", "  A backend conference. ");

        Assert.NotEqual(Guid.Empty, domainEvent.Id);
        Assert.Equal("Architecture Conference", domainEvent.Title);
        Assert.Equal("A backend conference.", domainEvent.Description);
        Assert.Equal(EventStatus.Draft, domainEvent.Status);
    }

    [Fact]
    public void UpdateDetails_WhenDraft_ShouldUpdateEvent()
    {
        DomainEvent domainEvent = DomainEvent.Create("Old", "Old description");

        domainEvent.UpdateDetails("New", "New description");

        Assert.Equal("New", domainEvent.Title);
        Assert.Equal("New description", domainEvent.Description);
    }

    [Fact]
    public void Publish_ShouldChangeDraftToPublished()
    {
        DomainEvent domainEvent = DomainEvent.Create("Architecture Conference", string.Empty);

        domainEvent.Publish();

        Assert.Equal(EventStatus.Published, domainEvent.Status);
    }

    [Fact]
    public void UpdateDetails_WhenPublished_ShouldFail()
    {
        DomainEvent domainEvent = DomainEvent.Create("Architecture Conference", string.Empty);
        domainEvent.Publish();

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => domainEvent.UpdateDetails("Changed", string.Empty)
        );

        Assert.Equal("Only a draft event can be edited.", exception.Message);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldBeIdempotent()
    {
        DomainEvent domainEvent = DomainEvent.Create("Architecture Conference", string.Empty);

        domainEvent.Cancel();
        domainEvent.Cancel();

        Assert.Equal(EventStatus.Cancelled, domainEvent.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutTitle_ShouldFail(string title)
    {
        DomainRuleException exception = Assert.Throws<DomainRuleException>(() => DomainEvent.Create(title, string.Empty));

        Assert.Equal("An event title is required.", exception.Message);
    }
}
