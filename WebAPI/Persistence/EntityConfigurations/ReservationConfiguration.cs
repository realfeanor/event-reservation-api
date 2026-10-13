using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations").HasKey(reservation => reservation.Id);
        builder.Property(reservation => reservation.Id).IsRequired();
        builder.Property(reservation => reservation.CustomerId).IsRequired();
        builder.Property(reservation => reservation.SessionId).IsRequired();
        builder.Property(reservation => reservation.Status).IsRequired();
        builder.Property(reservation => reservation.ExpiresAtUtc).IsRequired();
        builder.Property(reservation => reservation.ConfirmedAtUtc);
        builder.Property(reservation => reservation.CancelledAtUtc);
        builder.Property(reservation => reservation.ExpiredAtUtc);
        builder.Property(reservation => reservation.ConcurrencyToken).IsRequired().IsConcurrencyToken();
        builder.Property(reservation => reservation.CreatedDate).IsRequired();
        builder.Property(reservation => reservation.UpdatedDate);
        builder.Property(reservation => reservation.DeletedDate);
        builder.HasOne<User>().WithMany().HasForeignKey(reservation => reservation.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Session>().WithMany().HasForeignKey(reservation => reservation.SessionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(reservation => reservation.Seats).WithOne().HasForeignKey(seat => seat.ReservationId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(reservation => reservation.Seats).HasField("_seats").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(reservation => reservation.DomainEvents);
        builder.HasIndex(reservation => new { reservation.CustomerId, reservation.Status });
        builder.HasQueryFilter(reservation => !reservation.DeletedDate.HasValue);
        builder.HasBaseType((string)null!);
    }
}
