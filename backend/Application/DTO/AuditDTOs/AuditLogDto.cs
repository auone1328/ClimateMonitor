namespace Application.DTO.AuditDTOs
{
    public record AuditLogDto(
        Guid Id,
        Guid UserId,
        string UserEmail,
        string UserName,
        string ActionType,
        string Details,
        Guid? RoomId,
        string? RoomName,
        Guid? DeviceId,
        string? DeviceMac,
        DateTime Timestamp
    );
}
