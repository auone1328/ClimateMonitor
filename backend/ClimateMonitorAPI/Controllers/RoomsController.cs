using Application.Features.RoomFeatures.Create;
using Application.Features.RoomFeatures.GetList;
using Application.Features.RoomFeatures.GetById;
using Application.Features.RoomFeatures.SetTargetTemperature;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateMonitorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomsController(IMediator mediator) => _mediator = mediator;

    public record CreateRoomRequest(string Name, string? Description, float? TargetTemperature);
    public record SetTargetTemperatureRequest(float TargetTemperature);

    [HttpGet("/api/buildings/{buildingId:guid}/rooms")]
    public async Task<IActionResult> GetByBuilding([FromRoute] Guid buildingId)
    {
        var result = await _mediator.Send(new GetRoomsByBuildingQuery(buildingId));
        return Ok(result);
    }

    [HttpGet("{roomId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid roomId)
    {
        var result = await _mediator.Send(new GetRoomByIdQuery(roomId));
        return Ok(result);
    }

    [HttpPost("/api/buildings/{buildingId:guid}/rooms")]
    public async Task<IActionResult> Create([FromRoute] Guid buildingId, [FromBody] CreateRoomRequest request)
    {
        var result = await _mediator.Send(new CreateRoomCommand(buildingId, request.Name, request.Description, request.TargetTemperature));
        return Ok(result);
    }

    [HttpPatch("{roomId:guid}/target-temperature")]
    public async Task<IActionResult> SetTargetTemperature([FromRoute] Guid roomId, [FromBody] SetTargetTemperatureRequest request)
    {
        var result = await _mediator.Send(new SetTargetTemperatureCommand(roomId, request.TargetTemperature));
        return Ok(result);
    }
}
