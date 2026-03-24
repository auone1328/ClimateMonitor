using Application.DTO.AuditDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.AuditFeatures.GetByBuilding
{
    public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
    {
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public GetAuditLogsQueryHandler(
            IAuditLogRepository auditLogRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _auditLogRepo = auditLogRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<IReadOnlyList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken ct)
        {
            var role = await _accessRightRepo.GetRoleAsync(_userContext.UserId, request.BuildingId);
            if (role != AccessRole.Admin)
                throw new BadRequestException("Admin access required");

            var logs = await _auditLogRepo.GetForBuildingAsync(request.BuildingId, request.FromUtc, request.ToUtc);
            return logs
                .Select(l =>
                {
                    var roomName = l.Room?.Name ?? l.Device?.Room?.Name;
                    var deviceMac = l.Device?.MacAddress;
                    var userEmail = l.User?.Email ?? string.Empty;
                    var userName = l.User?.UserName ?? string.Empty;
                    return new AuditLogDto(
                        l.Id,
                        l.UserId,
                        userEmail,
                        userName,
                        l.ActionType.ToString(),
                        l.Details,
                        l.RoomId,
                        roomName,
                        l.DeviceId,
                        deviceMac,
                        l.Timestamp);
                })
                .ToList();
        }
    }
}
