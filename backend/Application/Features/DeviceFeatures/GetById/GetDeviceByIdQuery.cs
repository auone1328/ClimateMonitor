using Application.DTO.DeviceDTOs;
using MediatR;

namespace Application.Features.DeviceFeatures.GetById
{
    public record GetDeviceByIdQuery(Guid DeviceId) : IRequest<DeviceDto>;
}
