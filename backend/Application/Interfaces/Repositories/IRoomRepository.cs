using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IRoomRepository
    {
        Task<Room?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Room>> GetByBuildingIdAsync(Guid buildingId);
        Task AddAsync(Room room);
        Task UpdateTargetTemperatureAsync(Room room, float targetTemperature);
    }
}
