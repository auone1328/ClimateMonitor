using MediatR;

namespace Application.Features.RegistrationFeatures.AcceptInvite
{
    public record AcceptAccessInviteAuthenticatedCommand(string Token) : IRequest<Unit>;
}
