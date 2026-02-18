using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public enum AccessRole { Admin, User, Observer }

    public class AccessRight
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid BuildingId { get; set; }
        public Building Building { get; set; } = null!;
        public AccessRole Role { get; set; }
    }
}
