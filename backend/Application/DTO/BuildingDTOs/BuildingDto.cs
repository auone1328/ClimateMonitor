namespace Application.DTO.BuildingDTOs
{
    public record BuildingDto(
        Guid Id,
        string Name,
        string? Address
    );
}
