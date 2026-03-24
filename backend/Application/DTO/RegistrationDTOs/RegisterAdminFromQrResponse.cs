namespace Application.DTO.RegistrationDTOs
{
    public record RegisterAdminFromQrResponse(
        Guid UserId,
        string Email,
        string AccessToken,
        string RefreshToken,
        Guid BuildingId,
        Guid RoomId,
        Guid DeviceId
    );
}
