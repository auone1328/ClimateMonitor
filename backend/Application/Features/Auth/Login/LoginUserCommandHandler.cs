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

namespace Application.Features.Auth.Login
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _tokenService;
        private readonly ILogger<LoginUserCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginUserCommandHandler(
            SignInManager<User> signInManager, 
            UserManager<User> userManager, 
            IJwtService tokenService,
            ILogger<LoginUserCommandHandler> logger,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) 
            {
                _logger.LogError("Invalid credentials");
                throw new UnauthorizedAccessException("Invalid credentials");
            }
                
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded) 
            {
                _logger.LogError("Invalid credentials");
                throw new UnauthorizedAccessException("Invalid credentials");
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

            _logger.LogInformation("User is Logged (userId: {UserId}, email: {UserEmail})", user.Id, user.Email);
            return new LoginUserResponse(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(60));
        }
    }
}
