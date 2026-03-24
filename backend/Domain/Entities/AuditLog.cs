using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public enum AuditActionType
    {
        SetTemperature,
        ToggleRelay,
        UpdateSchedule,
        RegisterDevice,
        UserLogin,
        Other,
        ToggleCooler
    }

    public class AuditLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public AuditActionType ActionType { get; set; }
        public string Details { get; set; } = string.Empty; 
        public Guid? RoomId { get; set; }
        public Room? Room { get; set; }
        public Guid? DeviceId { get; set; }
        public Device? Device { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
