using Application.DTO.DeviceDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.DeviceFeatures.ToggleCooler
{
    public class ToggleCoolerCommandHandler : IRequestHandler<ToggleCoolerCommand, DeviceDto>
    {
        private readonly IDeviceRepository _deviceRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly IMqttPublisher _mqttPublisher;

        public ToggleCoolerCommandHandler(
            IDeviceRepository deviceRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext,
            IAuditLogRepository auditLogRepo,
            IMqttPublisher mqttPublisher)
        {
            _deviceRepo = deviceRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
            _auditLogRepo = auditLogRepo;
            _mqttPublisher = mqttPublisher;
        }

        public async Task<DeviceDto> Handle(ToggleCoolerCommand request, CancellationToken ct)
        {
            var device = await _deviceRepo.GetByIdWithRoomAsync(request.DeviceId);
            if (device == null)
                throw new BadRequestException("Device not found");

            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, device.Room.BuildingId);
            if (role != AccessRole.Admin && role != AccessRole.User)
                throw new BadRequestException("Access denied");

            await _deviceRepo.UpdateCoolerStateAsync(device, request.CoolerState);
            await _mqttPublisher.PublishCoolerCommandAsync(device.MacAddress, request.CoolerState);

            await _auditLogRepo.AddAsync(new AuditLog
            {
                UserId = _userContext.UserId,
                ActionType = AuditActionType.ToggleCooler,
                Details = request.CoolerState ? "Охладитель включен" : "Охладитель выключен",
                DeviceId = device.Id
            });

            return new DeviceDto(device.Id, device.RoomId, device.MacAddress, device.RelayState, device.HeaterState, device.CoolerState, device.LastSeen);
        }
    }
}
