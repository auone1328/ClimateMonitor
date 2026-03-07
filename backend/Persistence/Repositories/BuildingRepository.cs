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
    public class BuildingRepository : IBuildingRepository
    {
        private readonly ClimateMonitorDbContext _context;
        public BuildingRepository(ClimateMonitorDbContext context) => _context = context;

        public async Task<Building?> GetByNameAsync(string name)
        {
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Name == name);
            return building;
        }

        public async Task AddAsync(Building building)
        {
            await _context.Buildings.AddAsync(building);
            await _context.SaveChangesAsync();
        }
    }
}
