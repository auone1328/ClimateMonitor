using Application.DTO.RoomDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.RoomFeatures.SetTargetTemperature
{
    public class SetTargetTemperatureCommandHandler : IRequestHandler<SetTargetTemperatureCommand, RoomDto>
    {
        private readonly IRoomRepository _roomRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;
        private readonly IAuditLogRepository _auditLogRepo;

        public SetTargetTemperatureCommandHandler(
            IRoomRepository roomRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext,
            IAuditLogRepository auditLogRepo)
        {
            _roomRepo = roomRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
            _auditLogRepo = auditLogRepo;
        }

        public async Task<RoomDto> Handle(SetTargetTemperatureCommand request, CancellationToken ct)
        {
            var room = await _roomRepo.GetByIdAsync(request.RoomId);
            if (room == null)
                throw new BadRequestException("Room not found");

            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, room.BuildingId);
            if (role != AccessRole.Admin && role != AccessRole.User)
                throw new BadRequestException("Access denied");

            await _roomRepo.UpdateTargetTemperatureAsync(room, request.TargetTemperature);

            await _auditLogRepo.AddAsync(new AuditLog
            {
                UserId = _userContext.UserId,
                ActionType = AuditActionType.SetTemperature,
                Details = $"Целевая температура установлена: {request.TargetTemperature}",
                RoomId = room.Id
            });

            return new RoomDto(room.Id, room.BuildingId, room.Name, room.Description, room.TargetTemperature);
        }
    }
}
