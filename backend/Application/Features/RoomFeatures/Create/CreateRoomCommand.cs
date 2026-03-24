using Application.DTO.RoomDTOs;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.RoomFeatures.Create
{
    public record CreateRoomCommand(
        Guid BuildingId,
        [Required] string Name,
        string? Description,
        float? TargetTemperature
    ) : IRequest<RoomDto>;
}
