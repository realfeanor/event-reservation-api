using Application.Services.Repositories;
using Core.Application.Pipelines.Authorization;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Reservations.Queries.GetMine;

public class GetMyReservationsQuery : IRequest<IReadOnlyCollection<MyReservationResponse>>, ISecuredRequest
{
    public Guid CustomerId { get; set; }
    public string[] Roles => [];

    public class Handler : IRequestHandler<GetMyReservationsQuery, IReadOnlyCollection<MyReservationResponse>>
    {
        private readonly IReservationRepository _reservationRepository;

        public Handler(IReservationRepository reservationRepository) =>
            _reservationRepository = reservationRepository;

        public async Task<IReadOnlyCollection<MyReservationResponse>> Handle(
            GetMyReservationsQuery request,
            CancellationToken cancellationToken
        )
        {
            IReadOnlyCollection<Reservation> reservations = await _reservationRepository.GetByCustomerAsync(
                request.CustomerId,
                cancellationToken
            );
            return reservations.Select(reservation => new MyReservationResponse(
                reservation.Id,
                reservation.SessionId,
                reservation.Status,
                reservation.ExpiresAtUtc,
                reservation.ConfirmedAtUtc,
                reservation.CancelledAtUtc,
                reservation.ExpiredAtUtc,
                reservation.Seats.Select(seat => seat.SessionSeatId).ToArray()
            )).ToArray();
        }
    }
}

public record MyReservationResponse(
    Guid Id,
    Guid SessionId,
    ReservationStatus Status,
    DateTime ExpiresAtUtc,
    DateTime? ConfirmedAtUtc,
    DateTime? CancelledAtUtc,
    DateTime? ExpiredAtUtc,
    IReadOnlyCollection<Guid> SessionSeatIds
);
