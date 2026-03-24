namespace Application.DTO.RegistrationDTOs
{
    public record AccessInviteDto(
        string Token,
        DateTime ExpiresAt
    );
}
