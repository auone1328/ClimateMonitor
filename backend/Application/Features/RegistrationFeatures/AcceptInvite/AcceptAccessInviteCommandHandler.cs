using Application.DTO.Auth;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Application.Features.RegistrationFeatures.AcceptInvite
{
    public class AcceptAccessInviteCommandHandler : IRequestHandler<AcceptAccessInviteCommand, RegisterUserResponse>
    {
        private readonly IAccessInviteRepository _inviteRepo;
        private readonly IAccessRightRepository _accessRightRepo;
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AcceptAccessInviteCommandHandler> _logger;

        public AcceptAccessInviteCommandHandler(
            IAccessInviteRepository inviteRepo,
            IAccessRightRepository accessRightRepo,
            UserManager<User> userManager,
            IJwtService jwtService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AcceptAccessInviteCommandHandler> logger)
        {
            _inviteRepo = inviteRepo;
            _accessRightRepo = accessRightRepo;
            _userManager = userManager;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<RegisterUserResponse> Handle(AcceptAccessInviteCommand request, CancellationToken ct)
        {
            var invite = await _inviteRepo.GetByTokenAsync(request.Token);
            if (invite == null)
                throw new BadRequestException("Invite not found");
            if (invite.UsedAt.HasValue || invite.ExpiresAt <= DateTime.UtcNow)
                throw new BadRequestException("Invite expired or already used");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                user = new User
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                    throw new BadRequestException(string.Join(", ", createResult.Errors.Select(e => e.Description)));

                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                var valid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!valid)
                    throw new BadRequestException("Invalid credentials");
            }

            if (!await _accessRightRepo.ExistsAsync(user.Id, invite.BuildingId))
            {
                await _accessRightRepo.AddAsync(new AccessRight
                {
                    UserId = user.Id,
                    BuildingId = invite.BuildingId,
                    Role = invite.Role
                });
            }

            await _inviteRepo.MarkUsedAsync(invite, user.Id);

            var (accessToken, refreshToken) = await _jwtService.GenerateJwtTokensAsync(user);
            SetRefreshCookie(refreshToken);

            _logger.LogInformation("Invite accepted (userId: {UserId}, buildingId: {BuildingId})", user.Id, invite.BuildingId);

            return new RegisterUserResponse(user.Id, user.Email!, accessToken, refreshToken);
        }

        private void SetRefreshCookie(string refreshToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            };

            httpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
