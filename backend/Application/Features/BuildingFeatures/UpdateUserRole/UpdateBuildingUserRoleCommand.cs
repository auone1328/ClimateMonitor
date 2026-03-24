using Domain.Entities;
using MediatR;

namespace Application.Features.BuildingFeatures.UpdateUserRole
{
    public record UpdateBuildingUserRoleCommand(Guid BuildingId, Guid UserId, AccessRole Role) : IRequest<Unit>;
}
