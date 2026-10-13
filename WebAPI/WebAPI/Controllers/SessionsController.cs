using Application.Features.Sessions.Commands.OpenReservations;
using Application.Features.Sessions.Commands.Schedule;
using Application.Features.Sessions.Queries.GetSeatLayout;
using Application.Features.Reservations.Commands.HoldSeats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SessionsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Schedule(ScheduleSessionCommand command)
    {
        ScheduledSessionResponse response = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetSeatLayout), new { sessionId = response.Id }, response);
    }

    [HttpPut("{sessionId:guid}/reservations/open")]
    public async Task<IActionResult> OpenReservations(Guid sessionId)
    {
        OpenedSessionReservationsResponse response = await Mediator.Send(
            new OpenSessionReservationsCommand { SessionId = sessionId }
        );
        return Ok(response);
    }

    [HttpGet("{sessionId:guid}/seats")]
    public async Task<IActionResult> GetSeatLayout(Guid sessionId)
    {
        GetSessionSeatLayoutResponse response = await Mediator.Send(
            new GetSessionSeatLayoutQuery { SessionId = sessionId }
        );
        return Ok(response);
    }

    [HttpPost("{sessionId:guid}/seat-holds")]
    [Authorize]
    public async Task<IActionResult> HoldSeats(Guid sessionId, HoldSeatsCommand command)
    {
        command.SessionId = sessionId;
        command.CustomerId = getUserIdFromRequest();
        HeldSeatsResponse response = await Mediator.Send(command);
        return Created($"/api/Reservations/{response.Id}", response);
    }
}
