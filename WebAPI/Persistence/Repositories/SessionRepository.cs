using Application.Services.Repositories;
using Core.Persistence.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class SessionRepository : EfRepositoryBase<Session, Guid, BaseDbContext>, ISessionRepository
{
    public SessionRepository(BaseDbContext context)
        : base(context) { }

    public async Task<Session> AddWithSeatsAsync(Session session, CancellationToken cancellationToken = default)
    {
        DateTime utcNow = DateTime.UtcNow;
        session.CreatedDate = utcNow;
        foreach (SessionSeat seat in session.Seats)
            seat.CreatedDate = utcNow;

        await Context.AddAsync(session, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return session;
    }
}
