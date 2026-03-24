using Application.DTO.DeviceDTOs;
using MediatR;

namespace Application.Features.DeviceFeatures.ToggleRelay
{
    public record ToggleRelayCommand(
        Guid DeviceId,
        bool RelayState
    ) : IRequest<DeviceDto>;
}
