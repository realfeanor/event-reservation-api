using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class ReservationSeatConfiguration : IEntityTypeConfiguration<ReservationSeat>
{
    public void Configure(EntityTypeBuilder<ReservationSeat> builder)
    {
        builder.ToTable("ReservationSeats").HasKey(seat => seat.Id);
        builder.Property(seat => seat.Id).IsRequired();
        builder.Property(seat => seat.ReservationId).IsRequired();
        builder.Property(seat => seat.SessionSeatId).IsRequired();
        builder.Property(seat => seat.CreatedDate).IsRequired();
        builder.Property(seat => seat.UpdatedDate);
        builder.Property(seat => seat.DeletedDate);
        builder.HasOne<SessionSeat>().WithMany().HasForeignKey(seat => seat.SessionSeatId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(seat => new { seat.ReservationId, seat.SessionSeatId }).IsUnique();
        builder.HasQueryFilter(seat => !seat.DeletedDate.HasValue);
        builder.HasBaseType((string)null!);
    }
}
