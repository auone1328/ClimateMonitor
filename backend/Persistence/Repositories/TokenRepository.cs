using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly ClimateMonitorDbContext _dbContext;

        public TokenRepository(ClimateMonitorDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task CleanOldTokensAsync(User user) 
        {
            var oldTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.RevokedAt.HasValue)
                .ToListAsync();

            _dbContext.RefreshTokens.RemoveRange(oldTokens);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddTokenAsync(RefreshToken refreshToken) 
        {
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeRefreshTokenAsync(string token, string reason = "Manual revocation")
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            if (refreshToken == null || !refreshToken.RevokedAt.HasValue) return;

            refreshToken.RevokedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> IsRefreshTokenValidAsync(string token, User user)
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            if (refreshToken == null) return false;

            return refreshToken.UserId == user.Id &&
                   !refreshToken.RevokedAt.HasValue &&
                   refreshToken.ExpiresAt > DateTime.UtcNow;
        }
    }
}
