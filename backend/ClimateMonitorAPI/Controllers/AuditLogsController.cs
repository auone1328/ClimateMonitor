using Application.Features.AuditFeatures.GetByBuilding;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateMonitorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("/api/buildings/{buildingId:guid}/audit")]
    public async Task<IActionResult> GetByBuilding(
        [FromRoute] Guid buildingId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(buildingId, fromUtc, toUtc));
        return Ok(result);
    }
}
