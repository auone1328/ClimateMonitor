using Application.DTO.NotificationDTOs;
using MediatR;

namespace Application.Features.NotificationFeatures.GetList
{
    public record GetNotificationsQuery() : IRequest<IReadOnlyList<NotificationDto>>;
}
