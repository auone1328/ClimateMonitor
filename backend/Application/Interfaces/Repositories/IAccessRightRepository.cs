using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IAccessRightRepository
    {
        Task<AccessRole?> GetRoleAsync(Guid userId, Guid buildingId);
        Task<AccessRight?> GetAsync(Guid userId, Guid buildingId);
        Task<IReadOnlyList<AccessRight>> GetForBuildingAsync(Guid buildingId);
        Task<IReadOnlyList<AccessRight>> GetForBuildingWithUsersAsync(Guid buildingId);
        Task<IReadOnlyList<AccessRight>> GetForUserAsync(Guid userId);
        Task<bool> ExistsAsync(Guid userId, Guid buildingId);
        Task AddAsync(AccessRight accessRight);
        Task UpdateAsync(AccessRight accessRight);
        Task DeleteAsync(AccessRight accessRight);
    }
}
