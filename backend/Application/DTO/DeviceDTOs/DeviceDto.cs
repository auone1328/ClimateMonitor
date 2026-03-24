namespace Application.DTO.DeviceDTOs
{
    public record DeviceDto(
        Guid Id,
        Guid RoomId,
        string MacAddress,
        bool RelayState,
        bool HeaterState,
        bool CoolerState,
        DateTime? LastSeen
    );
}
