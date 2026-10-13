using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Events;
using Xunit;

namespace Domain.UnitTests;

public class ReservationTests
{
    private static readonly DateTime NowUtc = new(2027, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Confirm_ShouldConfirmReservationAndSeats()
    {
        (Session session, Reservation reservation) = CreatePendingReservation();

        bool changed = reservation.Confirm(session.Seats, session.Period.StartUtc, NowUtc.AddMinutes(1));

        Assert.True(changed);
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(NowUtc.AddMinutes(1), reservation.ConfirmedAtUtc);
        Assert.All(session.Seats, seat => Assert.Equal(SessionSeatStatus.Confirmed, seat.Status));
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ShouldBeIdempotent()
    {
        (Session session, Reservation reservation) = CreatePendingReservation();
        reservation.Confirm(session.Seats, session.Period.StartUtc, NowUtc.AddMinutes(1));
        Guid tokenAfterConfirmation = reservation.ConcurrencyToken;

        bool changed = reservation.Confirm(session.Seats, session.Period.StartUtc, NowUtc.AddMinutes(2));

        Assert.False(changed);
        Assert.Equal(tokenAfterConfirmation, reservation.ConcurrencyToken);
        Assert.Equal(NowUtc.AddMinutes(1), reservation.ConfirmedAtUtc);
    }

    [Fact]
    public void StateChanges_ShouldRaiseEventsOnlyOnce()
    {
        (Session session, Reservation reservation) = CreatePendingReservation();
        Assert.IsType<SeatsHeldDomainEvent>(Assert.Single(reservation.DequeueDomainEvents()));
        reservation.Confirm(session.Seats, session.Period.StartUtc, NowUtc.AddMinutes(1));

        Assert.IsType<ReservationConfirmedDomainEvent>(Assert.Single(reservation.DequeueDomainEvents()));
        Assert.Empty(reservation.DequeueDomainEvents());
    }

    [Fact]
    public void Confirm_AfterHoldExpires_ShouldFail()
    {
        (Session session, Reservation reservation) = CreatePendingReservation();

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => reservation.Confirm(session.Seats, session.Period.StartUtc, reservation.ExpiresAtUtc)
        );

        Assert.Equal("An expired reservation cannot be confirmed.", exception.Message);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
    }

    [Fact]
    public void Cancel_ConfirmedReservation_ShouldReleaseSeatsAndBeIdempotent()
    {
        (Session session, Reservation reservation) = CreatePendingReservation();
        reservation.Confirm(session.Seats, session.Period.StartUtc, NowUtc.AddMinutes(1));

        Assert.True(reservation.Cancel(session.Seats, session.Period.StartUtc, NowUtc.AddMinutes(2)));
        Assert.False(reservation.Cancel(session.Seats, session.Period.StartUtc, NowUtc.AddMinutes(3)));

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.All(session.Seats, seat =>
        {
            Assert.Equal(SessionSeatStatus.Available, seat.Status);
            Assert.Null(seat.ReservationId);
        });
    }

    [Fact]
    public void Expire_PendingReservation_ShouldReleaseSeatsAndIgnoreRepeat()
    {
        (Session session, Reservation reservation) = CreatePendingReservation();

        Assert.True(reservation.Expire(session.Seats, reservation.ExpiresAtUtc));
        Assert.False(reservation.Expire(session.Seats, reservation.ExpiresAtUtc.AddMinutes(1)));

        Assert.Equal(ReservationStatus.Expired, reservation.Status);
        Assert.Equal(reservation.ExpiresAtUtc, reservation.ExpiredAtUtc);
        Assert.All(session.Seats, seat => Assert.Equal(SessionSeatStatus.Available, seat.Status));
    }

    [Fact]
    public void Cancel_AfterSessionStarts_ShouldFail()
    {
        (Session session, Reservation reservation) = CreatePendingReservation();

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => reservation.Cancel(session.Seats, session.Period.StartUtc, session.Period.StartUtc)
        );

        Assert.Equal("A reservation cannot be cancelled after the session has started.", exception.Message);
    }

    private static (Session Session, Reservation Reservation) CreatePendingReservation()
    {
        Venue venue = Venue.Create("Lifecycle Hall");
        venue.AddSeat("A 1");
        venue.AddSeat("A 2");
        Session session = Session.Schedule(
            Guid.NewGuid(),
            venue.Id,
            NowUtc.AddDays(1),
            NowUtc.AddDays(1).AddHours(2),
            venue.Seats
        );
        session.OpenReservations(NowUtc);
        Reservation reservation = session.HoldSeats(
            Guid.NewGuid(),
            session.Seats.Select(seat => seat.Id),
            NowUtc,
            NowUtc.AddMinutes(10)
        );
        return (session, reservation);
    }
}
