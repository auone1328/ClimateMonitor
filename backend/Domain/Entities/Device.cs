using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Device : BaseEntity
    {
        public Guid RoomId { get; set; }
        public Room Room { get; set; } = null!;
        public string MacAddress { get; set; } = string.Empty;
        public string? ChipId { get; set; }
        public string? RegistrationSecret { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastSeen { get; set; }
        public bool RelayState { get; set; } = false;
        public bool HeaterState { get; set; } = false;
        public bool CoolerState { get; set; } = false;
        public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
    }
}
