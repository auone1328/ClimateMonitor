using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.DeviceDTOs
{
    public record RegisterDeviceFromQrResponse(
        Guid BuildingId,
        Guid RoomId,
        Guid DeviceId,
        string BuildingName,
        string RoomName
    );
}
