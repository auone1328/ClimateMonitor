using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Device
{
    public class RegisterDeviceDto
    {
        public string MacAddress { get; set; } = null!;
        public string? ChipId { get; set; }
        public string BuildingName { get; set; } = null!;
        public string RoomName { get; set; } = null!;
    }
}
