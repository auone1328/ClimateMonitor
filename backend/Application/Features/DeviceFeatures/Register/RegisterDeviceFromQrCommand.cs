using Application.DTO.DeviceDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeviceFeatures.Register
{
    public record RegisterDeviceFromQrCommand(
        string MacAddress,
        string BuildingName,
        string RoomName
    ) : IRequest<RegisterDeviceFromQrResponse>;
}
