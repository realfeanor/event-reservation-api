using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events").HasKey(domainEvent => domainEvent.Id);
        builder.Property(domainEvent => domainEvent.Id).HasColumnName("Id").IsRequired();
        builder.Property(domainEvent => domainEvent.Title).HasMaxLength(Event.MaximumTitleLength).IsRequired();
        builder.Property(domainEvent => domainEvent.Description).HasMaxLength(Event.MaximumDescriptionLength).IsRequired();
        builder.Property(domainEvent => domainEvent.Status).IsRequired();
        builder.Property(domainEvent => domainEvent.CreatedDate).IsRequired();
        builder.Property(domainEvent => domainEvent.UpdatedDate);
        builder.Property(domainEvent => domainEvent.DeletedDate);
        builder.HasQueryFilter(domainEvent => !domainEvent.DeletedDate.HasValue);
        builder.HasBaseType((string)null!);
    }
}
