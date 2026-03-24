using Application.DTO.BuildingDTOs;
using MediatR;

namespace Application.Features.BuildingFeatures.GetList
{
    public record GetBuildingsQuery() : IRequest<IReadOnlyList<BuildingDto>>;
}
