using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.BuildingFeatures.UpdateUserRole
{
    public class UpdateBuildingUserRoleCommandHandler : IRequestHandler<UpdateBuildingUserRoleCommand, Unit>
    {
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IBuildingRepository _buildingRepo;
        private readonly IUserContext _userContext;

        public UpdateBuildingUserRoleCommandHandler(
            IAccessRightRepository accessRightRepo,
            IBuildingRepository buildingRepo,
            IUserContext userContext)
        {
            _accessRightRepo = accessRightRepo;
            _buildingRepo = buildingRepo;
            _userContext = userContext;
        }

        public async Task<Unit> Handle(UpdateBuildingUserRoleCommand request, CancellationToken ct)
        {
            var building = await _buildingRepo.GetByIdAsync(request.BuildingId);
            if (building == null)
                throw new BadRequestException("Building not found");

            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, building.Id);
            if (role != AccessRole.Admin)
                throw new BadRequestException("Admin access required");

            if (request.UserId == _userContext.UserId)
                throw new BadRequestException("Cannot change your own role");

            var access = await _accessRightRepo.GetAsync(request.UserId, building.Id);
            if (access == null)
                throw new BadRequestException("User not found in building");

            if (access.Role == AccessRole.Admin)
                throw new BadRequestException("Cannot change admin role");

            if (request.Role != AccessRole.User && request.Role != AccessRole.Observer)
                throw new BadRequestException("Invalid role");

            access.Role = request.Role;
            await _accessRightRepo.UpdateAsync(access);
            return Unit.Value;
        }
    }
}
