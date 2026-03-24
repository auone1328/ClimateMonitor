using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ClimateMonitorDbContext _context;

        public NotificationRepository(ClimateMonitorDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Notification> notifications)
        {
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Notification>> GetForUserAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task MarkAsReadAsync(Notification notification)
        {
            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteOlderThanAsync(DateTime cutoffUtc)
        {
            return await _context.Notifications
                .Where(n => n.CreatedAt < cutoffUtc)
                .ExecuteDeleteAsync();
        }
    }
}
