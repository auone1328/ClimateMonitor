using Application.DTO.RoomDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.RoomFeatures.Create
{
    public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
    {
        private readonly IBuildingRepository _buildingRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public CreateRoomCommandHandler(
            IBuildingRepository buildingRepo,
            IRoomRepository roomRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _buildingRepo = buildingRepo;
            _roomRepo = roomRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken ct)
        {
            var building = await _buildingRepo.GetByIdAsync(request.BuildingId);
            if (building == null)
                throw new BadRequestException("Building not found");

            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, building.Id);
            if (role != AccessRole.Admin)
                throw new BadRequestException("Admin access required");

            var room = new Room
            {
                BuildingId = building.Id,
                Name = request.Name,
                Description = request.Description,
                TargetTemperature = request.TargetTemperature ?? 22.0f
            };

            await _roomRepo.AddAsync(room);

            return new RoomDto(room.Id, room.BuildingId, room.Name, room.Description, room.TargetTemperature);
        }
    }
}
