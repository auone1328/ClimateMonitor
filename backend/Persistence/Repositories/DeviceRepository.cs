using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Persistence.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly ClimateMonitorDbContext _context;

        public DeviceRepository(ClimateMonitorDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Device device)
        {
            await _context.Devices.AddAsync(device);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByMacAsync(string mac)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.MacAddress == mac);
            if (device == null) 
            {
                return false;
            }
            return true;
        }
    }
}
