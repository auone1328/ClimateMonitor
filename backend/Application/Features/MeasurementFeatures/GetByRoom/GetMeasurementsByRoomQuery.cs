using Application.DTO.MeasurementDTOs;
using MediatR;

namespace Application.Features.MeasurementFeatures.GetByRoom
{
    public record GetMeasurementsByRoomQuery(
        Guid RoomId,
        DateTime? FromUtc,
        DateTime? ToUtc
    ) : IRequest<IReadOnlyList<MeasurementDto>>;
}
