using Application.DTO.Auth;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Auth.Register
{

        public record RegisterUserCommand(
            [Required]
            [EmailAddress]
            string Email,
            [Required]
            string UserName,
            [Required]
            string Password,
            string? Role = "Observer"      
        ) : IRequest<RegisterUserResponse>;
}
