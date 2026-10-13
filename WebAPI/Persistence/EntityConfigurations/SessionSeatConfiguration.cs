using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class SessionSeatConfiguration : IEntityTypeConfiguration<SessionSeat>
{
    public void Configure(EntityTypeBuilder<SessionSeat> builder)
    {
        builder.ToTable("SessionSeats").HasKey(seat => seat.Id);
        builder.Property(seat => seat.Id).IsRequired();
        builder.Property(seat => seat.SessionId).IsRequired();
        builder.Property(seat => seat.VenueSeatId).IsRequired();
        builder.Property(seat => seat.Label).HasMaxLength(50).IsRequired();
        builder.Property(seat => seat.Status).IsRequired();
        builder.Property(seat => seat.ReservationId);
        builder.Property(seat => seat.HoldExpiresAtUtc);
        builder.Property(seat => seat.ConcurrencyToken).IsRequired().IsConcurrencyToken();
        builder.Property(seat => seat.CreatedDate).IsRequired();
        builder.Property(seat => seat.UpdatedDate);
        builder.Property(seat => seat.DeletedDate);
        builder.HasIndex(seat => new { seat.SessionId, seat.VenueSeatId }).IsUnique();
        builder.HasQueryFilter(seat => !seat.DeletedDate.HasValue);
        builder.HasBaseType((string)null!);
    }
}
