using Domain.Exceptions;
using Domain.ValueObjects;
using Xunit;

namespace Domain.UnitTests;

public class SeatLabelTests
{
    [Fact]
    public void Create_ShouldNormalizeWhitespaceAndPreserveDisplayCasing()
    {
        SeatLabel label = SeatLabel.Create("  Main   A  12 ");

        Assert.Equal("Main A 12", label.Value);
        Assert.Equal("MAIN A 12", label.NormalizedValue);
    }

    [Fact]
    public void Labels_ShouldCompareByNormalizedValue()
    {
        SeatLabel first = SeatLabel.Create("Main A 12");
        SeatLabel second = SeatLabel.Create(" main   a 12 ");

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutValue_ShouldFail(string value)
    {
        DomainRuleException exception = Assert.Throws<DomainRuleException>(() => SeatLabel.Create(value));

        Assert.Equal("A seat label is required.", exception.Message);
    }
}
