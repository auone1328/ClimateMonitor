using Application.DTO.BuildingDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.BuildingFeatures.GetUsers
{
    public class GetBuildingUsersQueryHandler : IRequestHandler<GetBuildingUsersQuery, IReadOnlyList<BuildingUserDto>>
    {
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IBuildingRepository _buildingRepo;
        private readonly IUserContext _userContext;

        public GetBuildingUsersQueryHandler(
            IAccessRightRepository accessRightRepo,
            IBuildingRepository buildingRepo,
            IUserContext userContext)
        {
            _accessRightRepo = accessRightRepo;
            _buildingRepo = buildingRepo;
            _userContext = userContext;
        }

        public async Task<IReadOnlyList<BuildingUserDto>> Handle(GetBuildingUsersQuery request, CancellationToken ct)
        {
            var building = await _buildingRepo.GetByIdAsync(request.BuildingId);
            if (building == null)
                throw new BadRequestException("Building not found");

            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, building.Id);
            if (role != Domain.Entities.AccessRole.Admin)
                throw new BadRequestException("Admin access required");

            var access = await _accessRightRepo.GetForBuildingWithUsersAsync(building.Id);
            return access
                .Select(a => new BuildingUserDto(a.UserId, a.User.Email ?? string.Empty, a.User.UserName ?? string.Empty, a.Role.ToString()))
                .ToList();
        }
    }
}
