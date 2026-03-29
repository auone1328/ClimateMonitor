using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ClimateMonitorDbContext _context;

        public RoomRepository(ClimateMonitorDbContext context) 
        {
            _context = context;
        }

        public async Task AddAsync(Room room)
        {
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
        }

        public async Task<Room?> GetByIdAsync(Guid id)
        {
            return await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IReadOnlyList<Room>> GetByBuildingIdAsync(Guid buildingId)
        {
            return await _context.Rooms
                .Where(r => r.BuildingId == buildingId)
                .ToListAsync();
        }

        public async Task UpdateTargetTemperatureAsync(Room room, float targetTemperature)
        {
            room.TargetTemperature = targetTemperature;
            room.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAutoControlEnabledAsync(Room room, bool enabled)
        {
            room.AutoControlEnabled = enabled;
            room.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
