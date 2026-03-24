using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.NotificationFeatures.MarkRead
{
    public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Unit>
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserContext _userContext;

        public MarkNotificationReadCommandHandler(
            INotificationRepository notificationRepo,
            IUserContext userContext)
        {
            _notificationRepo = notificationRepo;
            _userContext = userContext;
        }

        public async Task<Unit> Handle(MarkNotificationReadCommand request, CancellationToken ct)
        {
            var notification = await _notificationRepo.GetByIdAsync(request.NotificationId);
            if (notification == null)
                throw new BadRequestException("Notification not found");

            if (notification.UserId != _userContext.UserId)
                throw new BadRequestException("Access denied");

            await _notificationRepo.MarkAsReadAsync(notification);
            return Unit.Value;
        }
    }
}
