using Application.DTO.DeviceDTOs;
using Application.Features.DeviceFeatures.Register;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

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
}