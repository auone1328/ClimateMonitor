using Application.Features.MeasurementFeatures.Create;
using Application.Features.MeasurementFeatures.GetByRoom;
using Application.Features.MeasurementFeatures.GetLatest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateMonitorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MeasurementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeasurementsController(IMediator mediator) => _mediator = mediator;

    public record CreateMeasurementRequest(
        float Temperature,
        float Humidity,
        float CO2,
        bool RelayState,
        bool HeaterState,
        bool CoolerState,
        DateTime? Timestamp);

    [HttpPost("/api/devices/{deviceId:guid}/measurements")]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromRoute] Guid deviceId, [FromBody] CreateMeasurementRequest request)
    {
        var result = await _mediator.Send(new CreateMeasurementCommand(
            deviceId,
            request.Temperature,
            request.Humidity,
            request.CO2,
            request.RelayState,
            request.HeaterState,
            request.CoolerState,
            request.Timestamp));
        return Ok(result);
    }

    [HttpGet("/api/rooms/{roomId:guid}/measurements")]
    [Authorize]
    public async Task<IActionResult> GetByRoom(
        [FromRoute] Guid roomId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc)
    {
        var result = await _mediator.Send(new GetMeasurementsByRoomQuery(roomId, fromUtc, toUtc));
        return Ok(result);
    }

    [HttpGet("/api/rooms/{roomId:guid}/measurements/latest")]
    [Authorize]
    public async Task<IActionResult> GetLatest([FromRoute] Guid roomId)
    {
        var result = await _mediator.Send(new GetLatestMeasurementQuery(roomId));
        return Ok(result);
    }
}
