using Application.DTO.RegistrationDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.RegistrationFeatures.CreateInvite
{
    public class CreateAccessInviteCommandHandler : IRequestHandler<CreateAccessInviteCommand, AccessInviteDto>
    {
        private readonly IAccessInviteRepository _inviteRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public CreateAccessInviteCommandHandler(
            IAccessInviteRepository inviteRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _inviteRepo = inviteRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<AccessInviteDto> Handle(CreateAccessInviteCommand request, CancellationToken ct)
        {
            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, request.BuildingId);
            if (role != AccessRole.Admin)
                throw new BadRequestException("Admin access required");

            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddDays(request.ExpiresInDays ?? 7);

            var invite = new AccessInvite
            {
                BuildingId = request.BuildingId,
                CreatedByUserId = _userContext.UserId,
                Role = request.Role,
                Token = token,
                ExpiresAt = expiresAt
            };

            await _inviteRepo.AddAsync(invite);

            return new AccessInviteDto(token, expiresAt);
        }
    }
}
