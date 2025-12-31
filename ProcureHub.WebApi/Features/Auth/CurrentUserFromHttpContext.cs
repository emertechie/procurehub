using System.Security.Claims;
using ProcureHub.Infrastructure.Authentication;

namespace ProcureHub.WebApi.Features.Auth;

public sealed class CurrentUserFromHttpContext : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserFromHttpContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal Principal =>
        _httpContextAccessor.HttpContext?.User
        ?? new ClaimsPrincipal(new ClaimsIdentity());

    public Guid? UserId
    {
        get
        {
            var id = Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var guid)
                ? guid
                : null;
        }
    }

    public IReadOnlyCollection<string> Roles =>
        Principal.FindAll(ClaimTypes.Role)
            .Select(r => r.Value)
            .ToArray();
}
