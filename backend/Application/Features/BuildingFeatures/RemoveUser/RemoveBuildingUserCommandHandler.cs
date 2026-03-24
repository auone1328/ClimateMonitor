using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.BuildingFeatures.RemoveUser
{
    public class RemoveBuildingUserCommandHandler : IRequestHandler<RemoveBuildingUserCommand, Unit>
    {
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IBuildingRepository _buildingRepo;
        private readonly IUserContext _userContext;

        public RemoveBuildingUserCommandHandler(
            IAccessRightRepository accessRightRepo,
            IBuildingRepository buildingRepo,
            IUserContext userContext)
        {
            _accessRightRepo = accessRightRepo;
            _buildingRepo = buildingRepo;
            _userContext = userContext;
        }

        public async Task<Unit> Handle(RemoveBuildingUserCommand request, CancellationToken ct)
        {
            var building = await _buildingRepo.GetByIdAsync(request.BuildingId);
            if (building == null)
                throw new BadRequestException("Building not found");

            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, building.Id);
            if (role != AccessRole.Admin)
                throw new BadRequestException("Admin access required");

            if (request.UserId == _userContext.UserId)
                throw new BadRequestException("Cannot remove yourself");

            var access = await _accessRightRepo.GetAsync(request.UserId, building.Id);
            if (access == null)
                throw new BadRequestException("User not found in building");

            if (access.Role == AccessRole.Admin)
                throw new BadRequestException("Cannot remove admin");

            await _accessRightRepo.DeleteAsync(access);
            return Unit.Value;
        }
    }
}
