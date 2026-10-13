using Application.Features.Events.Commands.Create;
using Application.Features.Events.Commands.Publish;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateEventCommand command)
    {
        CreatedEventResponse response = await Mediator.Send(command);
        return Created(string.Empty, response);
    }

    [HttpPut("{eventId:guid}/publish")]
    public async Task<IActionResult> Publish(Guid eventId)
    {
        PublishedEventResponse response = await Mediator.Send(new PublishEventCommand { EventId = eventId });
        return Ok(response);
    }
}
