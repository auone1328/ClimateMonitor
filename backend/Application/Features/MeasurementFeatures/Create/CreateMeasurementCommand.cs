using Application.DTO.MeasurementDTOs;
using MediatR;

namespace Application.Features.MeasurementFeatures.Create
{
    public record CreateMeasurementCommand(
        Guid DeviceId,
        float Temperature,
        float Humidity,
        float CO2,
        bool RelayState,
        bool HeaterState,
        bool CoolerState,
        DateTime? Timestamp
    ) : IRequest<MeasurementDto>;
}
