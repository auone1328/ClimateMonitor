using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    public class AccessInviteRepository : IAccessInviteRepository
    {
        private readonly ClimateMonitorDbContext _context;

        public AccessInviteRepository(ClimateMonitorDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AccessInvite invite)
        {
            await _context.AccessInvites.AddAsync(invite);
            await _context.SaveChangesAsync();
        }

        public async Task<AccessInvite?> GetByTokenAsync(string token)
        {
            return await _context.AccessInvites
                .Include(ai => ai.Building)
                .FirstOrDefaultAsync(ai => ai.Token == token);
        }

        public async Task MarkUsedAsync(AccessInvite invite, Guid usedByUserId)
        {
            invite.UsedAt = DateTime.UtcNow;
            invite.UsedByUserId = usedByUserId;
            await _context.SaveChangesAsync();
        }
    }
}
