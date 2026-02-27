using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Auth.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogoutCommandHandler(
            ITokenRepository tokenRepository,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _tokenRepository = tokenRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return Unit.Value;
            }

            var refreshToken = httpContext.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unit.Value;
            }

            // Отзываем токен в БД
            await _tokenRepository.RevokeRefreshTokenAsync(refreshToken, "User logout");

            // Удаляем cookie
            httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
            {                
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Path = "/",
            });

            return Unit.Value;
        }
    }
}
