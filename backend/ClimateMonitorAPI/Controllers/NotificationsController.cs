using Application.Features.NotificationFeatures.GetList;
using Application.Features.NotificationFeatures.MarkRead;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateMonitorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetNotificationsQuery());
        return Ok(result);
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead([FromRoute] Guid notificationId)
    {
        await _mediator.Send(new MarkNotificationReadCommand(notificationId));
        return NoContent();
    }
}
