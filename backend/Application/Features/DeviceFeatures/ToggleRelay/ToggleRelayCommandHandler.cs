using Application.DTO.DeviceDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.DeviceFeatures.ToggleRelay
{
    public class ToggleRelayCommandHandler : IRequestHandler<ToggleRelayCommand, DeviceDto>
    {
        private readonly IDeviceRepository _deviceRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly IMqttPublisher _mqttPublisher;

        public ToggleRelayCommandHandler(
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

        public async Task<DeviceDto> Handle(ToggleRelayCommand request, CancellationToken ct)
        {
            var device = await _deviceRepo.GetByIdWithRoomAsync(request.DeviceId);
            if (device == null)
                throw new BadRequestException("Device not found");

            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, device.Room.BuildingId);
            if (role != AccessRole.Admin && role != AccessRole.User)
                throw new BadRequestException("Access denied");

            await _deviceRepo.UpdateRelayStateAsync(device, request.RelayState);
            await _deviceRepo.UpdateHeaterStateAsync(device, request.RelayState);
            await _mqttPublisher.PublishHeaterCommandAsync(device.MacAddress, request.RelayState);

            await _auditLogRepo.AddAsync(new AuditLog
            {
                UserId = _userContext.UserId,
                ActionType = AuditActionType.ToggleRelay,
                Details = request.RelayState ? "Реле включено" : "Реле выключено",
                DeviceId = device.Id
            });

            return new DeviceDto(device.Id, device.RoomId, device.MacAddress, device.RelayState, device.HeaterState, device.CoolerState, device.LastSeen);
        }
    }
}
