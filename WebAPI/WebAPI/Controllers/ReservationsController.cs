using Application.Features.Reservations.Commands;
using Application.Features.Reservations.Commands.Cancel;
using Application.Features.Reservations.Commands.Confirm;
using Application.Features.Reservations.Queries.GetMine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReservationsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        IReadOnlyCollection<MyReservationResponse> response = await Mediator.Send(
            new GetMyReservationsQuery { CustomerId = getUserIdFromRequest() }
        );
        return Ok(response);
    }

    [HttpPut("{reservationId:guid}/confirmation")]
    public async Task<IActionResult> Confirm(Guid reservationId)
    {
        ReservationStateResponse response = await Mediator.Send(
            new ConfirmReservationCommand
            {
                ReservationId = reservationId,
                CustomerId = getUserIdFromRequest()
            }
        );
        return Ok(response);
    }

    [HttpDelete("{reservationId:guid}")]
    public async Task<IActionResult> Cancel(Guid reservationId)
    {
        ReservationStateResponse response = await Mediator.Send(
            new CancelReservationCommand
            {
                ReservationId = reservationId,
                CustomerId = getUserIdFromRequest()
            }
        );
        return Ok(response);
    }
}
