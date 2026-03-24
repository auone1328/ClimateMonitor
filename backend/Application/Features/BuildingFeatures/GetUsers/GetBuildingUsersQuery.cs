using MediatR;

namespace Application.Features.BuildingFeatures.GetUsers
{
    public record GetBuildingUsersQuery(Guid BuildingId) : IRequest<IReadOnlyList<Application.DTO.BuildingDTOs.BuildingUserDto>>;
}
