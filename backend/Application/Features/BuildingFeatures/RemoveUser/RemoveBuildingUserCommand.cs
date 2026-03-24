using MediatR;

namespace Application.Features.BuildingFeatures.RemoveUser
{
    public record RemoveBuildingUserCommand(Guid BuildingId, Guid UserId) : IRequest<Unit>;
}
