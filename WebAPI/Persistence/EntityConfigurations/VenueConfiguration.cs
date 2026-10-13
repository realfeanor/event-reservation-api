using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues").HasKey(venue => venue.Id);

        builder.Property(venue => venue.Id).HasColumnName("Id").IsRequired();
        builder.Property(venue => venue.Name).HasColumnName("Name").HasMaxLength(Venue.MaximumNameLength).IsRequired();
        builder.Property(venue => venue.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(venue => venue.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(venue => venue.DeletedDate).HasColumnName("DeletedDate");

        builder.HasQueryFilter(venue => !venue.DeletedDate.HasValue);
        builder.HasMany(venue => venue.Seats).WithOne().HasForeignKey(seat => seat.VenueId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(venue => venue.Seats).HasField("_seats").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasBaseType((string)null!);
    }
}
