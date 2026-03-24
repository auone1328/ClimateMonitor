using Application.DTO.RegistrationDTOs;
using Domain.Entities;
using MediatR;

namespace Application.Features.RegistrationFeatures.CreateInvite
{
    public record CreateAccessInviteCommand(
        Guid BuildingId,
        AccessRole Role,
        int? ExpiresInDays
    ) : IRequest<AccessInviteDto>;
}
