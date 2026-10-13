using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions").HasKey(session => session.Id);
        builder.Property(session => session.Id).IsRequired();
        builder.Property(session => session.EventId).IsRequired();
        builder.Property(session => session.VenueId).IsRequired();
        builder.Property(session => session.ReservationsOpen).IsRequired();
        builder.Property(session => session.CreatedDate).IsRequired();
        builder.Property(session => session.UpdatedDate);
        builder.Property(session => session.DeletedDate);
        builder.OwnsOne(
            session => session.Period,
            periodBuilder =>
            {
                periodBuilder.Property(period => period.StartUtc).HasColumnName("StartUtc").IsRequired();
                periodBuilder.Property(period => period.EndUtc).HasColumnName("EndUtc").IsRequired();
            }
        );
        builder.Navigation(session => session.Period).IsRequired();
        builder.HasOne<Event>().WithMany().HasForeignKey(session => session.EventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Venue>().WithMany().HasForeignKey(session => session.VenueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(session => session.Seats).WithOne().HasForeignKey(seat => seat.SessionId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(session => session.Seats).HasField("_seats").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasQueryFilter(session => !session.DeletedDate.HasValue);
        builder.HasBaseType((string)null!);
    }
}
