namespace Application.DTO.BuildingDTOs
{
    public record BuildingUserDto(
        Guid UserId,
        string Email,
        string UserName,
        string Role);
}
