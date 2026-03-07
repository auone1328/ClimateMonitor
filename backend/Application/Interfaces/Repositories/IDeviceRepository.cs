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
        Task<bool> ExistsByMacAsync(string mac);
        Task AddAsync(Device device);
    }
}
