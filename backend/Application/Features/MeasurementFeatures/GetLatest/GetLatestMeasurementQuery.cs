using Application.DTO.MeasurementDTOs;
using MediatR;

namespace Application.Features.MeasurementFeatures.GetLatest
{
    public record GetLatestMeasurementQuery(Guid RoomId) : IRequest<MeasurementDto?>;
}
