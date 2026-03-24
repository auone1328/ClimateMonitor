using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !user.Identity?.IsAuthenticated == true)
                    throw new UnauthorizedAccessException("User not authenticated");

                var idClaim = user.FindFirst("UserId")?.Value
                    ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (!Guid.TryParse(idClaim, out var id))
                    throw new InvalidOperationException("Invalid user ID in token");

                return id;
            }
        }

        public string? Email => _httpContextAccessor.HttpContext?.User.FindFirst("Email")?.Value
            ?? _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
