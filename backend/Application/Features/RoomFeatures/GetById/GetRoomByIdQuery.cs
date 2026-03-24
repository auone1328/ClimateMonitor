using Application.DTO.RoomDTOs;
using MediatR;

namespace Application.Features.RoomFeatures.GetById
{
    public record GetRoomByIdQuery(Guid RoomId) : IRequest<RoomDto>;
}
