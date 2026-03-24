using Application.DTO.BuildingDTOs;
using MediatR;

namespace Application.Features.BuildingFeatures.GetById
{
    public record GetBuildingByIdQuery(Guid BuildingId) : IRequest<BuildingDto>;
}
