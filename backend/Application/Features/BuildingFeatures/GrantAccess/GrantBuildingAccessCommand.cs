using Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.BuildingFeatures.GrantAccess
{
    public record GrantBuildingAccessCommand(
        Guid BuildingId,
        [Required][EmailAddress] string Email,
        AccessRole Role
    ) : IRequest<Unit>;
}
