using Application.DTO.BuildingDTOs;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.BuildingFeatures.Create
{
    public record CreateBuildingCommand(
        [Required] string Name,
        string? Address
    ) : IRequest<BuildingDto>;
}
