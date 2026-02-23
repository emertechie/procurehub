using Microsoft.AspNetCore.Components.Authorization;
using ProcureHub.Infrastructure.Authentication;

namespace ProcureHub.BlazorApp.Infrastructure.Authentication;

internal sealed class AuthStateCurrentUserProvider(AuthenticationStateProvider authenticationStateProvider)
    : ICurrentUserProvider
{
    public async Task<ICurrentUser> GetCurrentUserAsync()
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        return new ClaimsPrincipalCurrentUserAdapter(authState.User);
    }
}
