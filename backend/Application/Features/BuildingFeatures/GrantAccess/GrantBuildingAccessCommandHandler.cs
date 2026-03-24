using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.BuildingFeatures.GrantAccess
{
    public class GrantBuildingAccessCommandHandler : IRequestHandler<GrantBuildingAccessCommand, Unit>
    {
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IBuildingRepository _buildingRepo;
        private readonly IUserContext _userContext;
        private readonly UserManager<User> _userManager;

        public GrantBuildingAccessCommandHandler(
            IAccessRightRepository accessRightRepo,
            IBuildingRepository buildingRepo,
            IUserContext userContext,
            UserManager<User> userManager)
        {
            _accessRightRepo = accessRightRepo;
            _buildingRepo = buildingRepo;
            _userContext = userContext;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(GrantBuildingAccessCommand request, CancellationToken ct)
        {
            var building = await _buildingRepo.GetByIdAsync(request.BuildingId);
            if (building == null)
                throw new BadRequestException("Building not found");

            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, building.Id);
            if (role != AccessRole.Admin)
                throw new BadRequestException("Admin access required");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new BadRequestException("User not found");

            var existing = await _accessRightRepo.GetAsync(user.Id, building.Id);
            if (existing == null)
            {
                await _accessRightRepo.AddAsync(new AccessRight
                {
                    UserId = user.Id,
                    BuildingId = building.Id,
                    Role = request.Role
                });
            }
            else
            {
                existing.Role = request.Role;
                await _accessRightRepo.UpdateAsync(existing);
            }

            return Unit.Value;
        }
    }
}
