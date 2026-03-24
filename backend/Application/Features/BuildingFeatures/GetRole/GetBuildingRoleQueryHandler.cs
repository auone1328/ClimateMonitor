using Application.DTO.BuildingDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.BuildingFeatures.GetRole
{
    public class GetBuildingRoleQueryHandler : IRequestHandler<GetBuildingRoleQuery, BuildingRoleDto>
    {
        private readonly IBuildingRepository _buildingRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public GetBuildingRoleQueryHandler(
            IBuildingRepository buildingRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _buildingRepo = buildingRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<BuildingRoleDto> Handle(GetBuildingRoleQuery request, CancellationToken ct)
        {
            var building = await _buildingRepo.GetByIdAsync(request.BuildingId);
            if (building == null)
                throw new BadRequestException("Building not found");

            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, request.BuildingId);
            if (role == null)
                throw new BadRequestException("Access denied");

            return new BuildingRoleDto(role.ToString());
        }
    }
}
