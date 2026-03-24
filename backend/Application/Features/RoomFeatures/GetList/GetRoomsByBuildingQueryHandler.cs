using Application.DTO.RoomDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.RoomFeatures.GetList
{
    public class GetRoomsByBuildingQueryHandler : IRequestHandler<GetRoomsByBuildingQuery, IReadOnlyList<RoomDto>>
    {
        private readonly IRoomRepository _roomRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public GetRoomsByBuildingQueryHandler(
            IRoomRepository roomRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _roomRepo = roomRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<IReadOnlyList<RoomDto>> Handle(GetRoomsByBuildingQuery request, CancellationToken ct)
        {
            var hasAccess = await _accessRightRepo.ExistsAsync(_userContext.UserId, request.BuildingId);
            if (!hasAccess)
                throw new BadRequestException("Access denied");

            var rooms = await _roomRepo.GetByBuildingIdAsync(request.BuildingId);
            return rooms
                .Select(r => new RoomDto(r.Id, r.BuildingId, r.Name, r.Description, r.TargetTemperature))
                .ToList();
        }
    }
}
