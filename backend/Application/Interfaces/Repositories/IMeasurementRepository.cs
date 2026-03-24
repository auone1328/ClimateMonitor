using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IMeasurementRepository
    {
        Task AddAsync(Measurement measurement);
        Task<IReadOnlyList<Measurement>> GetByRoomIdAsync(Guid roomId, DateTime? fromUtc, DateTime? toUtc);
        Task<Measurement?> GetLatestByRoomIdAsync(Guid roomId);
        Task<int> DeleteOlderThanAsync(DateTime cutoffUtc);
    }
}
