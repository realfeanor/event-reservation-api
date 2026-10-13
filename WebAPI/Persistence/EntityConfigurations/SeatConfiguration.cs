using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats").HasKey(seat => seat.Id);

        builder.Property(seat => seat.Id).HasColumnName("Id").IsRequired();
        builder.Property(seat => seat.VenueId).HasColumnName("VenueId").IsRequired();
        builder.Property(seat => seat.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(seat => seat.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(seat => seat.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(seat => seat.DeletedDate).HasColumnName("DeletedDate");

        builder.OwnsOne(
            seat => seat.Label,
            labelBuilder =>
            {
                labelBuilder.Property(label => label.Value).HasColumnName("Label").HasMaxLength(SeatLabel.MaximumLength).IsRequired();
                labelBuilder
                    .Property(label => label.NormalizedValue)
                    .HasColumnName("NormalizedLabel")
                    .HasMaxLength(SeatLabel.MaximumLength)
                    .IsRequired();
            }
        );
        builder.Navigation(seat => seat.Label).IsRequired();
        builder.HasQueryFilter(seat => !seat.DeletedDate.HasValue);

        builder.HasBaseType((string)null!);
    }
}
