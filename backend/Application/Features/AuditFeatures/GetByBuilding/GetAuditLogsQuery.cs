using Application.DTO.AuditDTOs;
using MediatR;

namespace Application.Features.AuditFeatures.GetByBuilding
{
    public record GetAuditLogsQuery(
        Guid BuildingId,
        DateTime? FromUtc,
        DateTime? ToUtc
    ) : IRequest<IReadOnlyList<AuditLogDto>>;
}
