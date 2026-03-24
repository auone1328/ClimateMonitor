using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    public class MeasurementRepository : IMeasurementRepository
    {
        private readonly ClimateMonitorDbContext _context;

        public MeasurementRepository(ClimateMonitorDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Measurement measurement)
        {
            await _context.Measurements.AddAsync(measurement);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Measurement>> GetByRoomIdAsync(Guid roomId, DateTime? fromUtc, DateTime? toUtc)
        {
            var query = _context.Measurements
                .Where(m => m.Device.RoomId == roomId)
                .AsQueryable();

            if (fromUtc.HasValue)
                query = query.Where(m => m.Timestamp >= fromUtc.Value);

            if (toUtc.HasValue)
                query = query.Where(m => m.Timestamp <= toUtc.Value);

            return await query
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<Measurement?> GetLatestByRoomIdAsync(Guid roomId)
        {
            return await _context.Measurements
                .Where(m => m.Device.RoomId == roomId)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<int> DeleteOlderThanAsync(DateTime cutoffUtc)
        {
            return await _context.Measurements
                .Where(m => m.Timestamp < cutoffUtc)
                .ExecuteDeleteAsync();
        }
    }
}
