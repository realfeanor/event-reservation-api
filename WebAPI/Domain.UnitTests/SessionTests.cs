using Domain.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Domain.UnitTests;

public class SessionTests
{
    private static readonly DateTime StartUtc = new(2027, 1, 10, 18, 0, 0, DateTimeKind.Utc);

    private static Venue CreateVenueWithSeat()
    {
        Venue venue = Venue.Create("Concert Hall");
        venue.AddSeat("Main A 1");
        return venue;
    }

    [Fact]
    public void Schedule_ShouldCreateClosedSession()
    {
        Guid eventId = Guid.NewGuid();
        Venue venue = CreateVenueWithSeat();

        Session session = Session.Schedule(eventId, venue.Id, StartUtc, StartUtc.AddHours(2), venue.Seats);

        Assert.Equal(eventId, session.EventId);
        Assert.Equal(venue.Id, session.VenueId);
        Assert.False(session.ReservationsOpen);
        Assert.Single(session.Seats);
        Assert.Equal("Main A 1", session.Seats.Single().Label);
    }

    [Fact]
    public void OpenReservations_BeforeStart_ShouldAllowReservations()
    {
        Venue venue = CreateVenueWithSeat();
        Session session = Session.Schedule(Guid.NewGuid(), venue.Id, StartUtc, StartUtc.AddHours(2), venue.Seats);

        session.OpenReservations(StartUtc.AddDays(-1));

        Assert.True(session.ReservationsOpen);
        Assert.True(session.CanAcceptReservations(StartUtc.AddMinutes(-1)));
    }

    [Fact]
    public void OpenReservations_AfterStart_ShouldFail()
    {
        Venue venue = CreateVenueWithSeat();
        Session session = Session.Schedule(Guid.NewGuid(), venue.Id, StartUtc, StartUtc.AddHours(2), venue.Seats);

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => session.OpenReservations(StartUtc)
        );

        Assert.Equal("Reservations cannot be opened after the session has started.", exception.Message);
    }

    [Fact]
    public void CloseReservations_ShouldPreventNewReservations()
    {
        Venue venue = CreateVenueWithSeat();
        Session session = Session.Schedule(Guid.NewGuid(), venue.Id, StartUtc, StartUtc.AddHours(2), venue.Seats);
        session.OpenReservations(StartUtc.AddDays(-1));

        session.CloseReservations();

        Assert.False(session.CanAcceptReservations(StartUtc.AddMinutes(-1)));
    }

    [Fact]
    public void Schedule_WithoutEvent_ShouldFail()
    {
        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => Session.Schedule(Guid.Empty, Guid.NewGuid(), StartUtc, StartUtc.AddHours(2), CreateVenueWithSeat().Seats)
        );

        Assert.Equal("A session must reference an event.", exception.Message);
    }

    [Fact]
    public void Schedule_ShouldSnapshotOnlyActiveVenueSeats()
    {
        Venue venue = Venue.Create("Concert Hall");
        Seat inactiveSeat = venue.AddSeat("Main A 1");
        venue.AddSeat("Main A 2");
        venue.DeactivateSeat(inactiveSeat.Id);

        Session session = Session.Schedule(Guid.NewGuid(), venue.Id, StartUtc, StartUtc.AddHours(2), venue.Seats);

        Assert.Single(session.Seats);
        Assert.Equal("Main A 2", session.Seats.Single().Label);
    }

    [Fact]
    public void Schedule_WithoutActiveVenueSeats_ShouldFail()
    {
        Venue venue = Venue.Create("Concert Hall");

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => Session.Schedule(Guid.NewGuid(), venue.Id, StartUtc, StartUtc.AddHours(2), venue.Seats)
        );

        Assert.Equal("A session requires at least one active venue seat.", exception.Message);
    }

    [Fact]
    public void HoldSeats_ShouldCreatePendingReservationAndHoldEverySelectedSeat()
    {
        Venue venue = Venue.Create("Concert Hall");
        venue.AddSeat("Main A 1");
        venue.AddSeat("Main A 2");
        Session session = Session.Schedule(Guid.NewGuid(), venue.Id, StartUtc, StartUtc.AddHours(2), venue.Seats);
        DateTime utcNow = StartUtc.AddDays(-1);
        session.OpenReservations(utcNow);
        Guid[] seatIds = session.Seats.Select(seat => seat.Id).ToArray();

        Reservation reservation = session.HoldSeats(
            Guid.NewGuid(),
            seatIds,
            utcNow,
            utcNow.AddMinutes(10)
        );

        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Equal(seatIds.Order(), reservation.Seats.Select(seat => seat.SessionSeatId).Order());
        Assert.All(session.Seats, seat =>
        {
            Assert.Equal(SessionSeatStatus.Held, seat.Status);
            Assert.Equal(reservation.Id, seat.ReservationId);
            Assert.Equal(utcNow.AddMinutes(10), seat.HoldExpiresAtUtc);
        });
    }

    [Fact]
    public void HoldSeats_WhenOneSeatIsUnavailable_ShouldNotPartiallyHoldOtherSeats()
    {
        Venue venue = Venue.Create("Concert Hall");
        venue.AddSeat("Main A 1");
        venue.AddSeat("Main A 2");
        Session session = Session.Schedule(Guid.NewGuid(), venue.Id, StartUtc, StartUtc.AddHours(2), venue.Seats);
        DateTime utcNow = StartUtc.AddDays(-1);
        session.OpenReservations(utcNow);
        SessionSeat[] seats = session.Seats.ToArray();
        session.HoldSeats(Guid.NewGuid(), [seats[1].Id], utcNow, utcNow.AddMinutes(10));

        Assert.Throws<SeatUnavailableException>(
            () => session.HoldSeats(Guid.NewGuid(), [seats[0].Id, seats[1].Id], utcNow, utcNow.AddMinutes(10))
        );

        Assert.Equal(SessionSeatStatus.Available, seats[0].Status);
        Assert.Equal(SessionSeatStatus.Held, seats[1].Status);
    }

    [Fact]
    public void HoldSeats_AfterPreviousHoldExpires_ShouldReplaceTheHold()
    {
        Venue venue = CreateVenueWithSeat();
        Session session = Session.Schedule(Guid.NewGuid(), venue.Id, StartUtc, StartUtc.AddHours(2), venue.Seats);
        DateTime utcNow = StartUtc.AddDays(-1);
        session.OpenReservations(utcNow);
        SessionSeat seat = session.Seats.Single();
        Reservation first = session.HoldSeats(Guid.NewGuid(), [seat.Id], utcNow, utcNow.AddMinutes(10));

        Reservation second = session.HoldSeats(
            Guid.NewGuid(),
            [seat.Id],
            utcNow.AddMinutes(11),
            utcNow.AddMinutes(21)
        );

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(second.Id, seat.ReservationId);
    }
}
