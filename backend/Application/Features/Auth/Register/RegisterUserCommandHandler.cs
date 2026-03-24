using Application.DTO.Auth;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _tokenService;
        private readonly ILogger<RegisterUserCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RegisterUserCommandHandler(
            UserManager<User> userManager, 
            IJwtService tokenService,
            ILogger<RegisterUserCommandHandler> logger, 
            IHttpContextAccessor httpContextAccessor
        )
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var err = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError(err);
                throw new Exception(err);
            }

            if (!string.IsNullOrEmpty(request.Role))
            {
                result = await _userManager.AddToRoleAsync(user, request.Role);
            }

            var (accessToken, refreshToken) = await _tokenService.GenerateJwtTokensAsync(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            };

            _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            _logger.LogInformation("User is Registered (userId: {UserId}, email: {UserEmail})", user.Id, user.Email);
            return new RegisterUserResponse(user.Id, user.Email!, accessToken, refreshToken);
        }
    }
}
