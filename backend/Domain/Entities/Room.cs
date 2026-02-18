using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Room : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public Guid BuildingId { get; set; }
        public Building Building { get; set; } = null!;
        public string? Description { get; set; }
        public float TargetTemperature { get; set; } = 22.0f;
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
