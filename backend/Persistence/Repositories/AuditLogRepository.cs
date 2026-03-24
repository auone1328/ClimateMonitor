using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ClimateMonitorDbContext _context;

        public AuditLogRepository(ClimateMonitorDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AuditLog log)
        {
            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<AuditLog>> GetForBuildingAsync(Guid buildingId, DateTime? fromUtc, DateTime? toUtc)
        {
            var query = _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Room)
                .Include(al => al.Device)
                .ThenInclude(d => d.Room)
                .Where(al =>
                    (al.RoomId.HasValue && al.Room!.BuildingId == buildingId) ||
                    (al.DeviceId.HasValue && al.Device!.Room.BuildingId == buildingId))
                .AsQueryable();

            if (fromUtc.HasValue)
                query = query.Where(al => al.Timestamp >= fromUtc.Value);

            if (toUtc.HasValue)
                query = query.Where(al => al.Timestamp <= toUtc.Value);

            return await query
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }
    }
}
