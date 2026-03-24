namespace Application.DTO.RoomDTOs
{
    public record RoomDto(
        Guid Id,
        Guid BuildingId,
        string Name,
        string? Description,
        float TargetTemperature
    );
}
