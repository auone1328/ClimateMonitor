namespace Application.DTO.NotificationDTOs
{
    public record NotificationDto(
        Guid Id,
        string Message,
        string Type,
        bool IsRead,
        DateTime CreatedAt
    );
}
