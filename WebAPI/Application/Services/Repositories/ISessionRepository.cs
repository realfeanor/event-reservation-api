using Core.Persistence.Repositories;
using Domain.Entities;

namespace Application.Services.Repositories;

public interface ISessionRepository : IAsyncRepository<Session, Guid>, IRepository<Session, Guid>
{
    Task<Session> AddWithSeatsAsync(Session session, CancellationToken cancellationToken = default);
}
