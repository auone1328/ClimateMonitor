using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class AccessRightRepository : IAccessRightRepository
    {
        private readonly ClimateMonitorDbContext _context;

        public AccessRightRepository(ClimateMonitorDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AccessRight accessRight)
        {
            await _context.AccessRights.AddAsync(accessRight);
            await _context.SaveChangesAsync();
        }
    }
}
