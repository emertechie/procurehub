using System.Security.Claims;

namespace ProcureHub.Infrastructure.Authentication;

public sealed class ClaimsPrincipalCurrentUser(ClaimsPrincipal principal)
    : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var guid)
                ? guid
                : null;
        }
    }

    public bool IsInRole(string roleName) => principal.IsInRole(roleName);
}
