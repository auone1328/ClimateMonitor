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
        private readonly IMeasurementRepository _measurementRepo;
        private readonly IRoomRepository _roomRepo;

        public ToggleRelayCommandHandler(
            IDeviceRepository deviceRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext,
            IAuditLogRepository auditLogRepo,
            IMqttPublisher mqttPublisher,
            IMeasurementRepository measurementRepo,
            IRoomRepository roomRepo)
        {
            _deviceRepo = deviceRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
            _auditLogRepo = auditLogRepo;
            _mqttPublisher = mqttPublisher;
            _measurementRepo = measurementRepo;
            _roomRepo = roomRepo;
        }

        public async Task<DeviceDto> Handle(ToggleRelayCommand request, CancellationToken ct)
        {
            var device = await _deviceRepo.GetByIdWithRoomAsync(request.DeviceId);
            if (device == null)
                throw new BadRequestException("Device not found");

            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, device.Room.BuildingId);
            if (role != AccessRole.Admin && role != AccessRole.User)
                throw new BadRequestException("Access denied");

            if (request.RelayState)
            {
                const float hysteresis = 0.5f;
                var latest = await _measurementRepo.GetLatestByRoomIdAsync(device.RoomId);
                if (latest == null)
                    throw new BadRequestException("Нет актуальных измерений температуры.");

                if (latest.Temperature >= device.Room.TargetTemperature - hysteresis)
                    throw new BadRequestException("Нельзя включить обогреватель при текущей температуре.");
            }

            if (!request.RelayState && device.RelayState)
                await _roomRepo.UpdateAutoControlEnabledAsync(device.Room, false);

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
