using Application.DTO.DeviceDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.DeviceFeatures.GetByRoom
{
    public class GetDevicesByRoomQueryHandler : IRequestHandler<GetDevicesByRoomQuery, IReadOnlyList<DeviceDto>>
    {
        private readonly IDeviceRepository _deviceRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public GetDevicesByRoomQueryHandler(
            IDeviceRepository deviceRepo,
            IRoomRepository roomRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _deviceRepo = deviceRepo;
            _roomRepo = roomRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<IReadOnlyList<DeviceDto>> Handle(GetDevicesByRoomQuery request, CancellationToken ct)
        {
            var room = await _roomRepo.GetByIdAsync(request.RoomId);
            if (room == null)
                throw new BadRequestException("Room not found");

            var hasAccess = await _accessRightRepo.ExistsAsync(_userContext.UserId, room.BuildingId);
            if (!hasAccess)
                throw new BadRequestException("Access denied");

            var devices = await _deviceRepo.GetByRoomIdAsync(room.Id);
            if (devices.Count == 0)
            {
                // Fallback: handle duplicate rooms with same name in the same building
                devices = await _deviceRepo.GetByBuildingAndRoomNameAsync(room.BuildingId, room.Name);
            }
            return devices
                .Select(d => new DeviceDto(d.Id, d.RoomId, d.MacAddress, d.RelayState, d.HeaterState, d.CoolerState, d.LastSeen))
                .ToList();
        }
    }
}
