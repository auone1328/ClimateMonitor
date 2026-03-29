using Application.DTO.MeasurementDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.MeasurementFeatures.Create
{
    public class CreateMeasurementCommandHandler : IRequestHandler<CreateMeasurementCommand, MeasurementDto>
    {
        private readonly IDeviceRepository _deviceRepo;
        private readonly IMeasurementRepository _measurementRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IMqttPublisher _mqttPublisher;

        public CreateMeasurementCommandHandler(
            IDeviceRepository deviceRepo,
            IMeasurementRepository measurementRepo,
            INotificationRepository notificationRepo,
            IAccessRightRepository accessRightRepo,
            IMqttPublisher mqttPublisher)
        {
            _deviceRepo = deviceRepo;
            _measurementRepo = measurementRepo;
            _notificationRepo = notificationRepo;
            _accessRightRepo = accessRightRepo;
            _mqttPublisher = mqttPublisher;
        }

        public async Task<MeasurementDto> Handle(CreateMeasurementCommand request, CancellationToken ct)
        {
            var device = await _deviceRepo.GetByIdWithRoomAsync(request.DeviceId);
            if (device == null)
                throw new BadRequestException("Device not found");

            var timestamp = request.Timestamp ?? DateTime.UtcNow;
            var measurement = new Measurement
            {
                DeviceId = device.Id,
                Timestamp = timestamp,
                Temperature = request.Temperature,
                Humidity = request.Humidity,
                CO2 = request.CO2,
                RelayState = request.RelayState,
                HeaterState = request.HeaterState,
                CoolerState = request.CoolerState
            };

            await _measurementRepo.AddAsync(measurement);

            await _deviceRepo.UpdateLastSeenAsync(device, timestamp);
            await _deviceRepo.UpdateRelayStateAsync(device, request.RelayState);
            await _deviceRepo.UpdateHeaterStateAsync(device, request.HeaterState);
            await _deviceRepo.UpdateCoolerStateAsync(device, request.CoolerState);

            await ApplyAutoControlAsync(device, measurement);
            await CreateAnomalyNotificationsAsync(device, measurement);

            return new MeasurementDto(
                measurement.Id,
                measurement.DeviceId,
                measurement.Timestamp,
                measurement.Temperature,
                measurement.Humidity,
                measurement.CO2,
                measurement.RelayState,
                measurement.HeaterState,
                measurement.CoolerState);
        }

        private async Task CreateAnomalyNotificationsAsync(Device device, Measurement measurement)
        {
            var room = device.Room;
            var anomalies = new List<string>();

            if (measurement.CO2 >= 1000)
                anomalies.Add($"High CO2: {measurement.CO2}");

            if (measurement.Temperature >= room.TargetTemperature + 2 ||
                measurement.Temperature <= room.TargetTemperature - 2)
                anomalies.Add($"Temperature out of range: {measurement.Temperature}");

            if (measurement.Humidity >= 70)
                anomalies.Add($"High humidity: {measurement.Humidity}");

            if (anomalies.Count == 0)
                return;

            var accessRights = await _accessRightRepo.GetForBuildingAsync(room.BuildingId);
            if (accessRights.Count == 0)
                return;

            var message = $"Room {room.Name}: {string.Join("; ", anomalies)}";
            var notifications = accessRights
                .Select(ar => new Notification
                {
                    UserId = ar.UserId,
                    Message = message,
                    Type = NotificationType.Anomaly
                })
                .ToList();

            await _notificationRepo.AddRangeAsync(notifications);
        }

        private async Task ApplyAutoControlAsync(Device device, Measurement measurement)
        {
            var room = device.Room;
            if (!room.AutoControlEnabled)
                return;
            var target = room.TargetTemperature;
            const float hysteresis = 0.5f;

            bool desiredHeater = false;
            bool desiredCooler = false;

            if (measurement.Temperature < target - hysteresis)
            {
                desiredHeater = true;
            }
            else if (measurement.Temperature > target + hysteresis)
            {
                desiredCooler = true;
            }

            if (desiredHeater && device.CoolerState)
            {
                await _mqttPublisher.PublishCoolerCommandAsync(device.MacAddress, false);
                await _deviceRepo.UpdateCoolerStateAsync(device, false);
            }

            if (desiredCooler && (device.HeaterState || device.RelayState))
            {
                await _mqttPublisher.PublishHeaterCommandAsync(device.MacAddress, false);
                await _deviceRepo.UpdateHeaterStateAsync(device, false);
                await _deviceRepo.UpdateRelayStateAsync(device, false);
            }

            if (desiredHeater != device.HeaterState)
            {
                await _mqttPublisher.PublishHeaterCommandAsync(device.MacAddress, desiredHeater);
                await _deviceRepo.UpdateHeaterStateAsync(device, desiredHeater);
                await _deviceRepo.UpdateRelayStateAsync(device, desiredHeater);
            }

            if (desiredCooler != device.CoolerState)
            {
                await _mqttPublisher.PublishCoolerCommandAsync(device.MacAddress, desiredCooler);
                await _deviceRepo.UpdateCoolerStateAsync(device, desiredCooler);
            }
        }
    }
}
