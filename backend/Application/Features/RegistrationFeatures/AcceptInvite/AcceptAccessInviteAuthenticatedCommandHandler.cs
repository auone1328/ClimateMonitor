using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.RegistrationFeatures.AcceptInvite
{
    public class AcceptAccessInviteAuthenticatedCommandHandler
        : IRequestHandler<AcceptAccessInviteAuthenticatedCommand, Unit>
    {
        private readonly IAccessInviteRepository _inviteRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;
        private readonly UserManager<Domain.Entities.User> _userManager;

        public AcceptAccessInviteAuthenticatedCommandHandler(
            IAccessInviteRepository inviteRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext,
            UserManager<Domain.Entities.User> userManager)
        {
            _inviteRepo = inviteRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(AcceptAccessInviteAuthenticatedCommand request, CancellationToken ct)
        {
            var invite = await _inviteRepo.GetByTokenAsync(request.Token);
            if (invite == null)
                throw new BadRequestException("Приглашение не найдено.");
            if (invite.ExpiresAt <= DateTime.UtcNow)
                throw new BadRequestException("Срок действия приглашения истек.");

            var userId = _userContext.UserId;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new BadRequestException("Пользователь не найден.");

            if (invite.UsedAt.HasValue)
            {
                if (invite.UsedByUserId == user.Id)
                    return Unit.Value;

                throw new BadRequestException("Приглашение уже использовано.");
            }

            if (!await _accessRightRepo.ExistsAsync(user.Id, invite.BuildingId))
            {
                await _accessRightRepo.AddAsync(new Domain.Entities.AccessRight
                {
                    UserId = user.Id,
                    BuildingId = invite.BuildingId,
                    Role = invite.Role
                });
            }

            await _inviteRepo.MarkUsedAsync(invite, user.Id);

            return Unit.Value;
        }
    }
}
