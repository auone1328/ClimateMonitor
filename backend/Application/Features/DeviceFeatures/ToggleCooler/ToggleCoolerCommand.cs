using Application.DTO.DeviceDTOs;
using MediatR;

namespace Application.Features.DeviceFeatures.ToggleCooler
{
    public record ToggleCoolerCommand(
        Guid DeviceId,
        bool CoolerState
    ) : IRequest<DeviceDto>;
}
