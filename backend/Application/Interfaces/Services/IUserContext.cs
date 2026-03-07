using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IUserContext
    {
        Guid UserId { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
    }
}
