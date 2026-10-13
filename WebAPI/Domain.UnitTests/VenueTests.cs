using Domain.Exceptions;
using Domain.Entities;
using Xunit;

namespace Domain.UnitTests;

public class VenueTests
{
    [Fact]
    public void Create_ShouldNormalizeName()
    {
        Venue venue = Venue.Create("  Istanbul Concert Hall  ");

        Assert.NotEqual(Guid.Empty, venue.Id);
        Assert.Equal("Istanbul Concert Hall", venue.Name);
        Assert.Empty(venue.Seats);
    }

    [Fact]
    public void Rename_ShouldChangeNormalizedName()
    {
        Venue venue = Venue.Create("Old Name");

        venue.Rename("  New Name ");

        Assert.Equal("New Name", venue.Name);
    }

    [Fact]
    public void AddSeat_ShouldCreateActiveSeatOwnedByVenue()
    {
        Venue venue = Venue.Create("Istanbul Concert Hall");

        Seat seat = venue.AddSeat("Main A 12");

        Assert.NotEqual(Guid.Empty, seat.Id);
        Assert.Equal(venue.Id, seat.VenueId);
        Assert.Equal("Main A 12", seat.Label.Value);
        Assert.True(seat.IsActive);
        Assert.Contains(seat, venue.Seats);
    }

    [Fact]
    public void AddSeat_WithDuplicateActiveLabel_ShouldFail()
    {
        Venue venue = Venue.Create("Istanbul Concert Hall");
        venue.AddSeat("Main A 12");

        DomainRuleException exception = Assert.Throws<DomainRuleException>(() => venue.AddSeat(" main   a 12 "));

        Assert.Equal("An active seat with label 'main a 12' already exists in this venue.", exception.Message);
        Assert.Single(venue.Seats);
    }

    [Fact]
    public void DeactivateSeat_ShouldMakeSeatInactive()
    {
        Venue venue = Venue.Create("Istanbul Concert Hall");
        Seat seat = venue.AddSeat("Main A 12");

        venue.DeactivateSeat(seat.Id);

        Assert.False(seat.IsActive);
    }

    [Fact]
    public void AddSeat_AfterPreviousSeatWithSameLabelWasDeactivated_ShouldCreateNewSeat()
    {
        Venue venue = Venue.Create("Istanbul Concert Hall");
        Seat oldSeat = venue.AddSeat("Main A 12");
        venue.DeactivateSeat(oldSeat.Id);

        Seat replacementSeat = venue.AddSeat("main a 12");

        Assert.NotEqual(oldSeat.Id, replacementSeat.Id);
        Assert.False(oldSeat.IsActive);
        Assert.True(replacementSeat.IsActive);
        Assert.Equal(2, venue.Seats.Count);
    }

    [Fact]
    public void DeactivateSeat_FromAnotherVenue_ShouldFailWithoutChangingSeats()
    {
        Venue venue = Venue.Create("Istanbul Concert Hall");
        Seat seat = venue.AddSeat("Main A 12");

        DomainRuleException exception = Assert.Throws<DomainRuleException>(() => venue.DeactivateSeat(Guid.NewGuid()));

        Assert.Equal("The seat does not belong to this venue.", exception.Message);
        Assert.True(seat.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutName_ShouldFail(string name)
    {
        DomainRuleException exception = Assert.Throws<DomainRuleException>(() => Venue.Create(name));

        Assert.Equal("A venue name is required.", exception.Message);
    }
}
