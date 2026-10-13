using Core.Persistence.Repositories;
using Domain.Entities;

namespace Application.Services.Repositories;

public interface IEventRepository : IAsyncRepository<Event, Guid>, IRepository<Event, Guid> { }
