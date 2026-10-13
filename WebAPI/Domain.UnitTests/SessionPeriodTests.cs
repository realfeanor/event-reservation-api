using Domain.Exceptions;
using Domain.ValueObjects;
using Xunit;

namespace Domain.UnitTests;

public class SessionPeriodTests
{
    [Fact]
    public void Create_WithValidUtcRange_ShouldSucceed()
    {
        DateTime start = new(2027, 1, 10, 18, 0, 0, DateTimeKind.Utc);
        DateTime end = start.AddHours(2);

        SessionPeriod period = SessionPeriod.Create(start, end);

        Assert.Equal(start, period.StartUtc);
        Assert.Equal(end, period.EndUtc);
    }

    [Fact]
    public void Create_WithNonUtcValue_ShouldFail()
    {
        DateTime start = new(2027, 1, 10, 18, 0, 0, DateTimeKind.Local);

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => SessionPeriod.Create(start, start.AddHours(2))
        );

        Assert.Equal("Session start and end times must be UTC.", exception.Message);
    }

    [Fact]
    public void Create_WhenEndIsNotLaterThanStart_ShouldFail()
    {
        DateTime start = new(2027, 1, 10, 18, 0, 0, DateTimeKind.Utc);

        DomainRuleException exception = Assert.Throws<DomainRuleException>(() => SessionPeriod.Create(start, start));

        Assert.Equal("Session end time must be later than its start time.", exception.Message);
    }
}
