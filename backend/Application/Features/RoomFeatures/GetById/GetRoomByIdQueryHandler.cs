using Application.DTO.RoomDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.RoomFeatures.GetById
{
    public class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, RoomDto>
    {
        private readonly IRoomRepository _roomRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public GetRoomByIdQueryHandler(
            IRoomRepository roomRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _roomRepo = roomRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken ct)
        {
            var room = await _roomRepo.GetByIdAsync(request.RoomId);
            if (room == null)
                throw new BadRequestException("Room not found");

            var hasAccess = await _accessRightRepo.ExistsAsync(_userContext.UserId, room.BuildingId);
            if (!hasAccess)
                throw new BadRequestException("Access denied");

            return new RoomDto(room.Id, room.BuildingId, room.Name, room.Description, room.TargetTemperature);
        }
    }
}
