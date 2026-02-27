using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface ITokenRepository
    {
        Task CleanOldTokensAsync(User user);
        Task AddTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string reason = "Manual revocation");
        Task<bool> IsRefreshTokenValidAsync(string token, User user);
    }
}
