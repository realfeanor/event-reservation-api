using Application.Services.Repositories;
using Core.Persistence.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class EventRepository : EfRepositoryBase<Event, Guid, BaseDbContext>, IEventRepository
{
    public EventRepository(BaseDbContext context)
        : base(context) { }
}
