using Application.DTO.BuildingDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.BuildingFeatures.GetById
{
    public class GetBuildingByIdQueryHandler : IRequestHandler<GetBuildingByIdQuery, BuildingDto>
    {
        private readonly IBuildingRepository _buildingRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public GetBuildingByIdQueryHandler(
            IBuildingRepository buildingRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _buildingRepo = buildingRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<BuildingDto> Handle(GetBuildingByIdQuery request, CancellationToken ct)
        {
            var building = await _buildingRepo.GetByIdAsync(request.BuildingId);
            if (building == null)
                throw new BadRequestException("Building not found");

            var hasAccess = await _accessRightRepo.ExistsAsync(_userContext.UserId, building.Id);
            if (!hasAccess)
                throw new BadRequestException("Access denied");

            return new BuildingDto(building.Id, building.Name, building.Address);
        }
    }
}
