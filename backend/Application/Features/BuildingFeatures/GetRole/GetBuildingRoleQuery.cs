using Application.DTO.BuildingDTOs;
using MediatR;

namespace Application.Features.BuildingFeatures.GetRole
{
    public record GetBuildingRoleQuery(Guid BuildingId) : IRequest<BuildingRoleDto>;
}
