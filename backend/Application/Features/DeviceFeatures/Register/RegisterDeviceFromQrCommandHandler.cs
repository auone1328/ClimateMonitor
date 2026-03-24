using Application.DTO.DeviceDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeviceFeatures.Register
{
    public class RegisterDeviceFromQrCommandHandler
        : IRequestHandler<RegisterDeviceFromQrCommand, RegisterDeviceFromQrResponse>
    {
        private readonly IBuildingRepository _buildingRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;   // текущий авторизованный пользователь

        public RegisterDeviceFromQrCommandHandler(
            IBuildingRepository buildingRepo,
            IRoomRepository roomRepo,
            IDeviceRepository deviceRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _buildingRepo = buildingRepo;
            _roomRepo = roomRepo;
            _deviceRepo = deviceRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<RegisterDeviceFromQrResponse> Handle(
            RegisterDeviceFromQrCommand request, CancellationToken ct)
        {
            var userId = _userContext.UserId; // из JWT
            var mac = NormalizeMac(request.MacAddress);
            var buildingName = (request.BuildingName ?? string.Empty).Trim();
            var roomName = (request.RoomName ?? string.Empty).Trim();

            //Проверяем, не зарегистрировано ли уже устройство
            if (await _deviceRepo.ExistsByMacAsync(mac))
                throw new BadRequestException("Устройство с таким MAC уже зарегистрировано");

            //Создаём/находим Building
            var building = await _buildingRepo.GetByNameAsync(buildingName);
            if (building == null)
            {
                building = new Building
                {
                    Name = buildingName,
                    CreatedByUserId = userId
                };
                await _buildingRepo.AddAsync(building);
            }

            //Создаём Room
            var room = new Room
            {
                Name = roomName,
                BuildingId = building.Id,
                TargetTemperature = 22.0f
            };
            await _roomRepo.AddAsync(room);

            //Создаём Device
            var device = new Device
            {
                RoomId = room.Id,
                MacAddress = mac,
                RegisteredAt = DateTime.UtcNow
            };
            await _deviceRepo.AddAsync(device);

            //Назначаем текущего пользователя BuildingAdmin
            var accessRight = new AccessRight
            {
                UserId = userId,
                BuildingId = building.Id,
                Role = AccessRole.Admin   // BuildingAdmin
            };
            await _accessRightRepo.AddAsync(accessRight);

            return new RegisterDeviceFromQrResponse(
                building.Id, room.Id, device.Id, building.Name, room.Name);
        }

        private static string NormalizeMac(string mac)
        {
            return mac.ToLowerInvariant().Replace(":", "").Replace("-", "");
        }
    }
}
