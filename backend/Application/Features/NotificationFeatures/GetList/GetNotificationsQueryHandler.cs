using Application.DTO.NotificationDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.NotificationFeatures.GetList
{
    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserContext _userContext;

        public GetNotificationsQueryHandler(
            INotificationRepository notificationRepo,
            IUserContext userContext)
        {
            _notificationRepo = notificationRepo;
            _userContext = userContext;
        }

        public async Task<IReadOnlyList<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
        {
            var notifications = await _notificationRepo.GetForUserAsync(_userContext.UserId);
            return notifications
                .Select(n => new NotificationDto(n.Id, TranslateMessage(n.Message), TranslateType(n.Type), n.IsRead, n.CreatedAt))
                .ToList();
        }

        private static string TranslateType(Domain.Entities.NotificationType type)
        {
            return type switch
            {
                Domain.Entities.NotificationType.Anomaly => "Аномалия",
                Domain.Entities.NotificationType.Alert => "Предупреждение",
                _ => "Уведомление"
            };
        }

        private static string TranslateMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return message;

            var result = message;
            if (result.StartsWith("Room ", StringComparison.OrdinalIgnoreCase))
            {
                result = "Комната " + result.Substring(5);
            }

            result = result.Replace("High CO2", "Высокий CO2");
            result = result.Replace("High humidity", "Высокая влажность");
            result = result.Replace("Temperature out of range", "Температура вне диапазона");
            result = result.Replace("Temperature", "Температура");
            result = result.Replace("humidity", "влажность");
            result = result.Replace("CO2", "CO2");

            return result;
        }
    }
}
