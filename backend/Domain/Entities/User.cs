using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public ICollection<Building> CreatedBuildings { get; set; } = new List<Building>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<AccessRight> AccessRights { get; set; } = new List<AccessRight>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}