using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log);
        Task<IReadOnlyList<AuditLog>> GetForBuildingAsync(Guid buildingId, DateTime? fromUtc, DateTime? toUtc);
    }
}
