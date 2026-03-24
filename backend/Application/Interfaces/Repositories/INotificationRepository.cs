using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task AddRangeAsync(IEnumerable<Notification> notifications);
        Task<IReadOnlyList<Notification>> GetForUserAsync(Guid userId);
        Task<Notification?> GetByIdAsync(Guid id);
        Task MarkAsReadAsync(Notification notification);
        Task<int> DeleteOlderThanAsync(DateTime cutoffUtc);
    }
}
