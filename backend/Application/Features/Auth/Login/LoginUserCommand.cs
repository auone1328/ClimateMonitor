using Application.DTO.Auth;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Auth.Login
{
    public record LoginUserCommand(
        [Required]
        [EmailAddress]
        string Email,
        [Required]
        string Password) : IRequest<LoginUserResponse>;
}
