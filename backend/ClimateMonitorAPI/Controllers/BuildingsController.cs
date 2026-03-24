using Application.Features.BuildingFeatures.Create;
using Application.Features.BuildingFeatures.GetList;
using Application.Features.BuildingFeatures.GetById;
using Application.Features.BuildingFeatures.GetRole;
using Application.Features.BuildingFeatures.GrantAccess;
using Application.Features.BuildingFeatures.GetUsers;
using Application.Features.BuildingFeatures.UpdateUserRole;
using Application.Features.BuildingFeatures.RemoveUser;
using Application.Features.RegistrationFeatures.CreateInvite;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateMonitorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BuildingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BuildingsController(IMediator mediator) => _mediator = mediator;

    public record CreateBuildingRequest(string Name, string? Address);
    public record GrantAccessRequest(string Email, AccessRole Role);
    public record CreateInviteRequest(AccessRole? Role, int? ExpiresInDays);
    public record UpdateUserRoleRequest(AccessRole Role);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetBuildingsQuery());
        return Ok(result);
    }

    [HttpGet("{buildingId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid buildingId)
    {
        var result = await _mediator.Send(new GetBuildingByIdQuery(buildingId));
        return Ok(result);
    }

    [HttpGet("{buildingId:guid}/role")]
    public async Task<IActionResult> GetRole([FromRoute] Guid buildingId)
    {
        var result = await _mediator.Send(new GetBuildingRoleQuery(buildingId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBuildingRequest request)
    {
        var result = await _mediator.Send(new CreateBuildingCommand(request.Name, request.Address));
        return Ok(result);
    }

    [HttpPost("{buildingId:guid}/access")]
    public async Task<IActionResult> GrantAccess([FromRoute] Guid buildingId, [FromBody] GrantAccessRequest request)
    {
        await _mediator.Send(new GrantBuildingAccessCommand(buildingId, request.Email, request.Role));
        return NoContent();
    }

    [HttpGet("{buildingId:guid}/users")]
    public async Task<IActionResult> GetUsers([FromRoute] Guid buildingId)
    {
        var result = await _mediator.Send(new GetBuildingUsersQuery(buildingId));
        return Ok(result);
    }

    [HttpPatch("{buildingId:guid}/users/{userId:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(
        [FromRoute] Guid buildingId,
        [FromRoute] Guid userId,
        [FromBody] UpdateUserRoleRequest request)
    {
        await _mediator.Send(new UpdateBuildingUserRoleCommand(buildingId, userId, request.Role));
        return NoContent();
    }

    [HttpDelete("{buildingId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveUser([FromRoute] Guid buildingId, [FromRoute] Guid userId)
    {
        await _mediator.Send(new RemoveBuildingUserCommand(buildingId, userId));
        return NoContent();
    }

    [HttpPost("{buildingId:guid}/invites")]
    public async Task<IActionResult> CreateInvite([FromRoute] Guid buildingId, [FromBody] CreateInviteRequest request)
    {
        var role = request.Role ?? AccessRole.User;
        var result = await _mediator.Send(new CreateAccessInviteCommand(buildingId, role, request.ExpiresInDays));
        return Ok(result);
    }
}
