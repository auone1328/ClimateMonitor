using Application.DTO.MeasurementDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.MeasurementFeatures.GetByRoom
{
    public class GetMeasurementsByRoomQueryHandler : IRequestHandler<GetMeasurementsByRoomQuery, IReadOnlyList<MeasurementDto>>
    {
        private readonly IRoomRepository _roomRepo;
        private readonly IMeasurementRepository _measurementRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public GetMeasurementsByRoomQueryHandler(
            IRoomRepository roomRepo,
            IMeasurementRepository measurementRepo,
            IAccessRightRepository accessRightRepo,
            IUserContext userContext)
        {
            _roomRepo = roomRepo;
            _measurementRepo = measurementRepo;
            _accessRightRepo = accessRightRepo;
            _userContext = userContext;
        }

        public async Task<IReadOnlyList<MeasurementDto>> Handle(GetMeasurementsByRoomQuery request, CancellationToken ct)
        {
            var room = await _roomRepo.GetByIdAsync(request.RoomId);
            if (room == null)
                throw new BadRequestException("Room not found");

            var hasAccess = await _accessRightRepo.ExistsAsync(_userContext.UserId, room.BuildingId);
            if (!hasAccess)
                throw new BadRequestException("Access denied");

            var measurements = await _measurementRepo.GetByRoomIdAsync(request.RoomId, request.FromUtc, request.ToUtc);
            return measurements
                .Select(m => new MeasurementDto(m.Id, m.DeviceId, m.Timestamp, m.Temperature, m.Humidity, m.CO2, m.RelayState, m.HeaterState, m.CoolerState))
                .ToList();
        }
    }
}
