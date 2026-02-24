using System.Security.Claims;
using ProcureHub.Infrastructure.Authentication;

namespace ProcureHub.WebApi.Features.Auth;

public sealed class HttpContextCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserProvider
{
    public Task<ICurrentUser> GetCurrentUserAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult<ICurrentUser>(new ClaimsPrincipalCurrentUserAdapter(principal));
    }
}
