using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IAccessInviteRepository
    {
        Task AddAsync(AccessInvite invite);
        Task<AccessInvite?> GetByTokenAsync(string token);
        Task MarkUsedAsync(AccessInvite invite, Guid usedByUserId);
    }
}
