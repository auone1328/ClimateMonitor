using MediatR;

namespace Application.Features.NotificationFeatures.MarkRead
{
    public record MarkNotificationReadCommand(Guid NotificationId) : IRequest<Unit>;
}
