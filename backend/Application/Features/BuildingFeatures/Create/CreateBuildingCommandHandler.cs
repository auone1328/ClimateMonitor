using Application.DTO.BuildingDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.BuildingFeatures.Create
{
    public class CreateBuildingCommandHandler : IRequestHandler<CreateBuildingCommand, BuildingDto>
    {
        private readonly IBuildingRepository _buildingRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public CreateBuildingCommandHandler(
            IBuildingRepository buildingRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _buildingRepo = buildingRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<BuildingDto> Handle(CreateBuildingCommand request, CancellationToken ct)
        {
            var userId = _userContext.UserId;

            var existing = await _buildingRepo.GetByNameAsync(request.Name);
            if (existing != null)
                throw new BadRequestException("Building with the same name already exists");

            var building = new Building
            {
                Name = request.Name,
                Address = request.Address,
                CreatedByUserId = userId
            };

            await _buildingRepo.AddAsync(building);

            if (!await _accessRightRepo.ExistsAsync(userId, building.Id))
            {
                await _accessRightRepo.AddAsync(new AccessRight
                {
                    UserId = userId,
                    BuildingId = building.Id,
                    Role = AccessRole.Admin
                });
            }

            return new BuildingDto(building.Id, building.Name, building.Address);
        }
    }
}
