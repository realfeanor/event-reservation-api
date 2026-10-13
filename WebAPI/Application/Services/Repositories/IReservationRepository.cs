using Core.Persistence.Repositories;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Repositories;

public interface IReservationRepository : IAsyncRepository<Reservation, Guid>, IRepository<Reservation, Guid>
{
    Task<Reservation> AddWithSeatsAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task SaveStateAsync(Reservation reservation, DateTime utcNow, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> GetExpiredPendingIdsAsync(
        DateTime utcNow,
        int batchSize,
        CancellationToken cancellationToken = default
    );
    Task<ReservationStatus?> GetStatusAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Reservation>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default
    );
}
