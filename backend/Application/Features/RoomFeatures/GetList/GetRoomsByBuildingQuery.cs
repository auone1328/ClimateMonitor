using Application.DTO.RoomDTOs;
using MediatR;

namespace Application.Features.RoomFeatures.GetList
{
    public record GetRoomsByBuildingQuery(Guid BuildingId) : IRequest<IReadOnlyList<RoomDto>>;
}
