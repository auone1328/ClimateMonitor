using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IDeviceRepository
    {
        Task<Device?> GetByIdAsync(Guid id);
        Task<Device?> GetByIdWithRoomAsync(Guid id);
        Task<IReadOnlyList<Device>> GetByRoomIdAsync(Guid roomId);
        Task<Device?> GetByMacAsync(string mac);
        Task<bool> ExistsByMacAsync(string mac);
        Task AddAsync(Device device);
        Task<IReadOnlyList<Device>> GetByBuildingAndRoomNameAsync(Guid buildingId, string roomName);
        Task UpdateRelayStateAsync(Device device, bool relayState);
        Task UpdateHeaterStateAsync(Device device, bool heaterState);
        Task UpdateCoolerStateAsync(Device device, bool coolerState);
        Task UpdateLastSeenAsync(Device device, DateTime lastSeen);
    }
}
