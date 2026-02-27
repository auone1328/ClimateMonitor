using Application.DTO.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;           
using System.Security.Claims;                          

namespace Application.Features.Auth.Refresh
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        private readonly IJwtService _tokenService;
        private readonly ITokenRepository _tokenRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RefreshTokenCommandHandler(
            IJwtService tokenService,
            UserManager<User> userManager,
            ITokenRepository tokenRepository,
            ILogger<RefreshTokenCommandHandler> logger, 
            IHttpContextAccessor httpContextAccessor
        )
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _tokenRepository = tokenRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) throw new InvalidOperationException("No HTTP context");
            
            var refreshToken = httpContext.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                throw new SecurityTokenException("Refresh token not found in cookies");

            var oldAccessToken = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var principal = _tokenService.GetPrincipalFromExpiredToken(oldAccessToken);
            if (principal == null)
            {
                _logger.LogError("Invalid access token");
                throw new SecurityTokenException("Invalid access token");
            }

            var userId = Guid.Parse(principal.FindFirstValue("UserId")!);
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null) 
            {
                _logger.LogError("User not found");
                throw new UnauthorizedAccessException("User not found");
            }
                
            var isValid = await _tokenRepository.IsRefreshTokenValidAsync(refreshToken, user);
            if (!isValid) 
            {
                _logger.LogError("Invalid or expired refresh token");
                throw new SecurityTokenException("Invalid or expired refresh token");
            }   

            // Генерируем новые токены (с ротацией)
            var (newAccessToken, newRefreshToken) = await _tokenService.GenerateJwtTokensAsync(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            };

            httpContext.Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

            _logger.LogInformation("Token is refreshed for (userId: {UserId}, email: {UserEmail})", user.Id, user.Email);
            return new RefreshTokenResponse(newAccessToken);
        }
    }
}
