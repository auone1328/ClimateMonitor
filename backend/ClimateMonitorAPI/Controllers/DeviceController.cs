using Application.DTO.Device;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace ClimateMonitorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = "Installer")]
public class DevicesController : ControllerBase
{
    private readonly ClimateMonitorDbContext _context;

    public DevicesController(ClimateMonitorDbContext context)
    {
        _context = context;
    }

    // POST: api/devices/register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MacAddress) 
            || string.IsNullOrWhiteSpace(dto.BuildingName) 
            || string.IsNullOrWhiteSpace(dto.RoomName))
            return BadRequest("Missing required fields");

        // Проверяем, существует ли устройство по Mac
        var existingDevice = await _context.Devices.FirstOrDefaultAsync(d => d.MacAddress == dto.MacAddress);
        if (existingDevice != null)
            return BadRequest("Device already registered");

        // Находим или создаём здание (пока без привязки к пользователю, позже доработаем)
        var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Name == dto.BuildingName);
        if (building == null)
        {
            building = new Building { Name = dto.BuildingName /*, CreatedByUserId = ... */ };
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();
        }

        // Находим или создаём комнату
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Name == dto.RoomName && r.BuildingId == building.Id);
        if (room == null)
        {
            room = new Room { Name = dto.RoomName, BuildingId = building.Id };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
        }

        // Создаём устройство
        var device = new Device
        {
            RoomId = room.Id,
            MacAddress = dto.MacAddress,
            ChipId = dto.ChipId,
            RegisteredAt = DateTime.UtcNow
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        // Возвращаем DeviceId для ESP
        return Ok(new { DeviceId = device.Id });
    }

    // Позже добавь другие методы, e.g., GET /devices/{id}, PUT /devices/{id}/relay (для управления реле)
}