using Application.DTO.DeviceDTOs;
using MediatR;

namespace Application.Features.DeviceFeatures.GetByRoom
{
    public record GetDevicesByRoomQuery(Guid RoomId) : IRequest<IReadOnlyList<DeviceDto>>;
}
