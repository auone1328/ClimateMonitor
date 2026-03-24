using Application.DTO.RegistrationDTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Application.Features.RegistrationFeatures.RegisterAdminFromDeviceQr
{
    public class RegisterAdminFromDeviceQrCommandHandler
        : IRequestHandler<RegisterAdminFromDeviceQrCommand, RegisterAdminFromQrResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RegisterAdminFromDeviceQrCommandHandler> _logger;
        private readonly IBuildingRepository _buildingRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IAccessRightRepository _accessRightRepo;

        public RegisterAdminFromDeviceQrCommandHandler(
            UserManager<User> userManager,
            IJwtService jwtService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RegisterAdminFromDeviceQrCommandHandler> logger,
            IBuildingRepository buildingRepo,
            IRoomRepository roomRepo,
            IDeviceRepository deviceRepo,
            IAccessRightRepository accessRightRepo)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _buildingRepo = buildingRepo;
            _roomRepo = roomRepo;
            _deviceRepo = deviceRepo;
            _accessRightRepo = accessRightRepo;
        }

        public async Task<RegisterAdminFromQrResponse> Handle(RegisterAdminFromDeviceQrCommand request, CancellationToken ct)
        {
            var mac = NormalizeMac(request.MacAddress);
            var buildingName = (request.BuildingName ?? string.Empty).Trim();
            var roomName = (request.RoomName ?? string.Empty).Trim();
            if (await _deviceRepo.ExistsByMacAsync(mac))
                throw new BadRequestException("Device already registered");

            var existingBuilding = await _buildingRepo.GetByNameAsync(buildingName);
            if (existingBuilding != null)
                throw new BadRequestException("Building with the same name already exists");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                user = new User
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            else
            {
                var valid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!valid)
                    throw new BadRequestException("Invalid credentials");
            }

            await _userManager.AddToRoleAsync(user, "BuildingAdmin");

            var building = new Building
            {
                Name = buildingName,
                CreatedByUserId = user.Id
            };
            await _buildingRepo.AddAsync(building);

            var room = new Room
            {
                Name = roomName,
                BuildingId = building.Id,
                TargetTemperature = 22.0f
            };
            await _roomRepo.AddAsync(room);

            var device = new Device
            {
                RoomId = room.Id,
                MacAddress = mac,
                RegisteredAt = DateTime.UtcNow,
                RegistrationSecret = request.Secret
            };
            await _deviceRepo.AddAsync(device);

            if (!await _accessRightRepo.ExistsAsync(user.Id, building.Id))
            {
                await _accessRightRepo.AddAsync(new AccessRight
                {
                    UserId = user.Id,
                    BuildingId = building.Id,
                    Role = AccessRole.Admin
                });
            }

            var (accessToken, refreshToken) = await _jwtService.GenerateJwtTokensAsync(user);
            SetRefreshCookie(refreshToken);

            _logger.LogInformation("Admin registered from device QR (userId: {UserId}, buildingId: {BuildingId})", user.Id, building.Id);

            return new RegisterAdminFromQrResponse(
                user.Id,
                user.Email!,
                accessToken,
                refreshToken,
                building.Id,
                room.Id,
                device.Id);
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

        private static string NormalizeMac(string mac)
        {
            return mac.ToLowerInvariant().Replace(":", "").Replace("-", "");
        }
    }
}
