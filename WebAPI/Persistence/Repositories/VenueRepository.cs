using Application.Services.Repositories;
using Core.Persistence.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class VenueRepository : EfRepositoryBase<Venue, Guid, BaseDbContext>, IVenueRepository
{
    public VenueRepository(BaseDbContext context)
        : base(context) { }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<Seat> newSeats = Context.ChangeTracker
            .Entries<Venue>()
            .SelectMany(entry => entry.Entity.Seats)
            .Where(seat => seat.CreatedDate == default)
            .ToArray();
        foreach (Seat seat in newSeats)
            Context.Add(seat);

        Context.ChangeTracker.DetectChanges();

        foreach (var entry in Context.ChangeTracker.Entries<IEntityTimestamps>())
        {
            if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added)
                entry.Entity.CreatedDate = DateTime.UtcNow;
            else if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Modified)
                entry.Entity.UpdatedDate = DateTime.UtcNow;
        }

        await Context.SaveChangesAsync(cancellationToken);
    }
}
