using Core.Persistence.Repositories;
using Domain.Entities;

namespace Application.Services.Repositories;

public interface IVenueRepository : IAsyncRepository<Venue, Guid>, IRepository<Venue, Guid>
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
