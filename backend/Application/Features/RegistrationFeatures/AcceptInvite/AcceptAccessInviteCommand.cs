using Application.DTO.Auth;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.RegistrationFeatures.AcceptInvite
{
    public record AcceptAccessInviteCommand(
        [Required] string Token,
        [Required][EmailAddress] string Email,
        [Required] string UserName,
        [Required] string Password
    ) : IRequest<RegisterUserResponse>;
}
