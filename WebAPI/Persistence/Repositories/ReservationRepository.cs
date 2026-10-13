using Application.Services.Repositories;
using Core.Persistence.Repositories;
using Domain.Entities;
using Persistence.Contexts;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

public class ReservationRepository : EfRepositoryBase<Reservation, Guid, BaseDbContext>, IReservationRepository
{
    public ReservationRepository(BaseDbContext context) : base(context) { }

    public async Task<Reservation> AddWithSeatsAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default
    )
    {
        DateTime utcNow = DateTime.UtcNow;
        reservation.CreatedDate = utcNow;
        foreach (ReservationSeat seat in reservation.Seats)
            seat.CreatedDate = utcNow;

        await Context.AddAsync(reservation, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    public async Task SaveStateAsync(
        Reservation reservation,
        DateTime utcNow,
        CancellationToken cancellationToken = default
    )
    {
        reservation.UpdatedDate = utcNow;
        foreach (var entry in Context.ChangeTracker.Entries<SessionSeat>().Where(entry => entry.State == EntityState.Modified))
            entry.Entity.UpdatedDate = utcNow;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetExpiredPendingIdsAsync(
        DateTime utcNow,
        int batchSize,
        CancellationToken cancellationToken = default
    ) => await Query()
        .AsNoTracking()
        .Where(reservation => reservation.Status == ReservationStatus.Pending && reservation.ExpiresAtUtc <= utcNow)
        .OrderBy(reservation => reservation.ExpiresAtUtc)
        .Select(reservation => reservation.Id)
        .Take(batchSize)
        .ToArrayAsync(cancellationToken);

    public async Task<ReservationStatus?> GetStatusAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default
    ) => await Query()
        .AsNoTracking()
        .Where(reservation => reservation.Id == reservationId)
        .Select(reservation => (ReservationStatus?)reservation.Status)
        .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Reservation>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default
    ) => await Query()
        .AsNoTracking()
        .Include(reservation => reservation.Seats)
        .Where(reservation => reservation.CustomerId == customerId)
        .OrderByDescending(reservation => reservation.CreatedDate)
        .ToArrayAsync(cancellationToken);
}
