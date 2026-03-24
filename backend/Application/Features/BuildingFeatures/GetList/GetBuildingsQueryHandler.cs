using Application.DTO.BuildingDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.BuildingFeatures.GetList
{
    public class GetBuildingsQueryHandler : IRequestHandler<GetBuildingsQuery, IReadOnlyList<BuildingDto>>
    {
        private readonly IBuildingRepository _buildingRepo;
        private readonly IUserContext _userContext;

        public GetBuildingsQueryHandler(IBuildingRepository buildingRepo, IUserContext userContext)
        {
            _buildingRepo = buildingRepo;
            _userContext = userContext;
        }

        public async Task<IReadOnlyList<BuildingDto>> Handle(GetBuildingsQuery request, CancellationToken ct)
        {
            var userId = _userContext.UserId;
            var buildings = await _buildingRepo.GetForUserAsync(userId);
            return buildings
                .Select(b => new BuildingDto(b.Id, b.Name, b.Address))
                .ToList();
        }
    }
}
