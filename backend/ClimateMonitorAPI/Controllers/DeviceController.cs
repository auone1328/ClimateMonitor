using Application.Features.DeviceFeatures.Register;
using Application.Features.DeviceFeatures.GetByRoom;
using Application.Features.DeviceFeatures.GetById;
using Application.Features.DeviceFeatures.ToggleRelay;
using Application.Features.DeviceFeatures.ToggleCooler;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateMonitorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DevicesController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register-from-qr")]
    public async Task<IActionResult> RegisterFromQr([FromBody] RegisterDeviceFromQrCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("/api/rooms/{roomId:guid}/devices")]
    public async Task<IActionResult> GetByRoom([FromRoute] Guid roomId)
    {
        var result = await _mediator.Send(new GetDevicesByRoomQuery(roomId));
        return Ok(result);
    }

    [HttpGet("{deviceId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid deviceId)
    {
        var result = await _mediator.Send(new GetDeviceByIdQuery(deviceId));
        return Ok(result);
    }

    public record ToggleRelayRequest(bool RelayState);
    public record ToggleCoolerRequest(bool CoolerState);

    [HttpPost("{deviceId:guid}/relay")]
    public async Task<IActionResult> ToggleRelay([FromRoute] Guid deviceId, [FromBody] ToggleRelayRequest request)
    {
        var result = await _mediator.Send(new ToggleRelayCommand(deviceId, request.RelayState));
        return Ok(result);
    }

    [HttpPost("{deviceId:guid}/cooler")]
    public async Task<IActionResult> ToggleCooler([FromRoute] Guid deviceId, [FromBody] ToggleCoolerRequest request)
    {
        var result = await _mediator.Send(new ToggleCoolerCommand(deviceId, request.CoolerState));
        return Ok(result);
    }
}
