using System.Reflection;
using Domain.Entities;
using Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services.DomainEvents;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task DispatchAndClearAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in reservation.DequeueDomainEvents())
        {
            Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
            MethodInfo handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!;
            foreach (object handler in handlers.OfType<object>())
            {
                Task task = (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
                await task;
            }
        }
    }
}
