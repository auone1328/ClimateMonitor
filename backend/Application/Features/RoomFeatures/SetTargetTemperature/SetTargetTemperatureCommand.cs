using Application.DTO.RoomDTOs;
using MediatR;

namespace Application.Features.RoomFeatures.SetTargetTemperature
{
    public record SetTargetTemperatureCommand(
        Guid RoomId,
        float TargetTemperature
    ) : IRequest<RoomDto>;
}
