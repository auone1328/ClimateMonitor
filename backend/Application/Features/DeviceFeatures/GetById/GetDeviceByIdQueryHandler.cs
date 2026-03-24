using Application.DTO.DeviceDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.DeviceFeatures.GetById
{
    public class GetDeviceByIdQueryHandler : IRequestHandler<GetDeviceByIdQuery, DeviceDto>
    {
        private readonly IDeviceRepository _deviceRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public GetDeviceByIdQueryHandler(
            IDeviceRepository deviceRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _deviceRepo = deviceRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<DeviceDto> Handle(GetDeviceByIdQuery request, CancellationToken ct)
        {
            var device = await _deviceRepo.GetByIdWithRoomAsync(request.DeviceId);
            if (device == null)
                throw new BadRequestException("Device not found");

            var hasAccess = await _accessRightRepo.ExistsAsync(_userContext.UserId, device.Room.BuildingId);
            if (!hasAccess)
                throw new BadRequestException("Access denied");

            return new DeviceDto(device.Id, device.RoomId, device.MacAddress, device.RelayState, device.HeaterState, device.CoolerState, device.LastSeen);
        }
    }
}
