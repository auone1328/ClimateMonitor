using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IBuildingRepository
    {
        Task<Building?> GetByNameAsync(string name);
        Task<Building?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Building>> GetForUserAsync(Guid userId);
        Task AddAsync(Building building);
    }
}
