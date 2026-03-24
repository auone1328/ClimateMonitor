using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Measurement : BaseEntity
    {
        public Guid DeviceId { get; set; }
        public Device Device { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float CO2 { get; set; }
        public bool RelayState { get; set; }
        public bool HeaterState { get; set; }
        public bool CoolerState { get; set; }
    }
}
