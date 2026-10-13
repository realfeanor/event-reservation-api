using Application.Features.Venues.Commands.AddSeat;
using Application.Features.Venues.Commands.Create;
using Application.Features.Venues.Commands.DeactivateSeat;
using Application.Features.Venues.Queries.GetById;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VenuesController : BaseController
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        GetVenueByIdResponse result = await Mediator.Send(new GetVenueByIdQuery { Id = id });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVenueCommand command)
    {
        CreatedVenueResponse result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{venueId:guid}/seats")]
    public async Task<IActionResult> AddSeat(Guid venueId, [FromBody] AddSeatToVenueCommand command)
    {
        command.VenueId = venueId;
        AddedSeatToVenueResponse result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = venueId }, result);
    }

    [HttpDelete("{venueId:guid}/seats/{seatId:guid}")]
    public async Task<IActionResult> DeactivateSeat(Guid venueId, Guid seatId)
    {
        DeactivatedVenueSeatResponse result = await Mediator.Send(
            new DeactivateVenueSeatCommand { VenueId = venueId, SeatId = seatId }
        );
        return Ok(result);
    }
}
