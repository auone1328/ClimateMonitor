using Application.DTO.MeasurementDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.MeasurementFeatures.GetLatest
{
    public class GetLatestMeasurementQueryHandler : IRequestHandler<GetLatestMeasurementQuery, MeasurementDto?>
    {
        private readonly IRoomRepository _roomRepo;
        private readonly IMeasurementRepository _measurementRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly IUserContext _userContext;

        public GetLatestMeasurementQueryHandler(
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

        public async Task<MeasurementDto?> Handle(GetLatestMeasurementQuery request, CancellationToken ct)
        {
            var room = await _roomRepo.GetByIdAsync(request.RoomId);
            if (room == null)
                throw new BadRequestException("Room not found");

            var hasAccess = await _accessRightRepo.ExistsAsync(_userContext.UserId, room.BuildingId);
            if (!hasAccess)
                throw new BadRequestException("Access denied");

            var measurement = await _measurementRepo.GetLatestByRoomIdAsync(request.RoomId);
            if (measurement == null)
                return null;

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
    }
}
