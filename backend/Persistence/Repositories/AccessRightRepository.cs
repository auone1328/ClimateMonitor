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

        public async Task<AccessRight?> GetAsync(Guid userId, Guid buildingId)
        {
            return await _context.AccessRights
                .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.BuildingId == buildingId);
        }

        public async Task<AccessRole?> GetRoleAsync(Guid userId, Guid buildingId)
        {
            var access = await _context.AccessRights
                .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.BuildingId == buildingId);
            return access?.Role;
        }

        public async Task<IReadOnlyList<AccessRight>> GetForBuildingAsync(Guid buildingId)
        {
            return await _context.AccessRights
                .Where(ar => ar.BuildingId == buildingId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AccessRight>> GetForBuildingWithUsersAsync(Guid buildingId)
        {
            return await _context.AccessRights
                .Include(ar => ar.User)
                .Where(ar => ar.BuildingId == buildingId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AccessRight>> GetForUserAsync(Guid userId)
        {
            return await _context.AccessRights
                .Where(ar => ar.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid buildingId)
        {
            return await _context.AccessRights
                .AnyAsync(ar => ar.UserId == userId && ar.BuildingId == buildingId);
        }

        public async Task UpdateAsync(AccessRight accessRight)
        {
            accessRight.Building = null!;
            accessRight.User = null!;
            _context.AccessRights.Update(accessRight);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AccessRight accessRight)
        {
            _context.AccessRights.Remove(accessRight);
            await _context.SaveChangesAsync();
        }
    }
}
