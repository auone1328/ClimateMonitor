using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Persistence.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly ClimateMonitorDbContext _context;

        public DeviceRepository(ClimateMonitorDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Device device)
        {
            await _context.Devices.AddAsync(device);
            await _context.SaveChangesAsync();
        }

        public async Task<Device?> GetByIdAsync(Guid id)
        {
            return await _context.Devices.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Device?> GetByIdWithRoomAsync(Guid id)
        {
            return await _context.Devices
                .Include(d => d.Room)
                .ThenInclude(r => r.Building)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IReadOnlyList<Device>> GetByRoomIdAsync(Guid roomId)
        {
            return await _context.Devices
                .Where(d => d.RoomId == roomId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Device>> GetByBuildingAndRoomNameAsync(Guid buildingId, string roomName)
        {
            return await _context.Devices
                .Include(d => d.Room)
                .Where(d => d.Room.BuildingId == buildingId && EF.Functions.ILike(d.Room.Name, roomName))
                .ToListAsync();
        }

        public async Task<Device?> GetByMacAsync(string mac)
        {
            var normalized = NormalizeMac(mac);
            return await _context.Devices
                .FirstOrDefaultAsync(d =>
                    d.MacAddress.ToLower().Replace(":", "").Replace("-", "") == normalized);
        }

        public async Task<bool> ExistsByMacAsync(string mac)
        {
            var normalized = NormalizeMac(mac);
            return await _context.Devices
                .AnyAsync(d => d.MacAddress.ToLower().Replace(":", "").Replace("-", "") == normalized);
        }

        public async Task UpdateRelayStateAsync(Device device, bool relayState)
        {
            device.RelayState = relayState;
            device.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateHeaterStateAsync(Device device, bool heaterState)
        {
            device.HeaterState = heaterState;
            device.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCoolerStateAsync(Device device, bool coolerState)
        {
            device.CoolerState = coolerState;
            device.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLastSeenAsync(Device device, DateTime lastSeen)
        {
            device.LastSeen = lastSeen;
            await _context.SaveChangesAsync();
        }

        private static string NormalizeMac(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
                return string.Empty;

            return mac
                .ToLowerInvariant()
                .Replace(":", "")
                .Replace("-", "")
                .Trim();
        }
    }
}
