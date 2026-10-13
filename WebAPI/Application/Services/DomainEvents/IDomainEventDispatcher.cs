using Domain.Entities;

namespace Application.Services.DomainEvents;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
