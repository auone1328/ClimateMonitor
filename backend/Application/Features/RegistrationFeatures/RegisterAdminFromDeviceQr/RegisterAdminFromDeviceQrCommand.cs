using Application.DTO.RegistrationDTOs;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.RegistrationFeatures.RegisterAdminFromDeviceQr
{
    public record RegisterAdminFromDeviceQrCommand(
        [Required] string MacAddress,
        [Required] string BuildingName,
        [Required] string RoomName,
        [Required] string Secret,
        [Required][EmailAddress] string Email,
        [Required] string UserName,
        [Required] string Password
    ) : IRequest<RegisterAdminFromQrResponse>;
}
