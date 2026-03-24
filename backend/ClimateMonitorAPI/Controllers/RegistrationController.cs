using Application.Features.RegistrationFeatures.RegisterAdminFromDeviceQr;
using Application.Features.RegistrationFeatures.AcceptInvite;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateMonitorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RegistrationController : ControllerBase
{
    private readonly IMediator _mediator;

    public RegistrationController(IMediator mediator) => _mediator = mediator;

    public record RegisterFromDeviceQrRequest(
        string MacAddress,
        string BuildingName,
        string RoomName,
        string Secret,
        string Email,
        string UserName,
        string Password);

    public record AcceptInviteRequest(string Token, string Email, string UserName, string Password);
    public record AcceptInviteAuthenticatedRequest(string Token);

    [HttpPost("device-qr")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterFromDeviceQr([FromBody] RegisterFromDeviceQrRequest request)
    {
        var result = await _mediator.Send(new RegisterAdminFromDeviceQrCommand(
            request.MacAddress,
            request.BuildingName,
            request.RoomName,
            request.Secret,
            request.Email,
            request.UserName,
            request.Password));
        return Ok(result);
    }

    [HttpPost("invite")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
    {
        var result = await _mediator.Send(new AcceptAccessInviteCommand(
            request.Token,
            request.Email,
            request.UserName,
            request.Password));
        return Ok(result);
    }

    [HttpPost("invite/authenticated")]
    [Authorize]
    public async Task<IActionResult> AcceptInviteAuthenticated([FromBody] AcceptInviteAuthenticatedRequest request)
    {
        await _mediator.Send(new AcceptAccessInviteAuthenticatedCommand(request.Token));
        return Ok();
    }
}
