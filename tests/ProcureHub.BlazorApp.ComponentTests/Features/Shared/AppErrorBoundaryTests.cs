using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Application.Common.Authorization;
using ProcureHub.BlazorApp.Components.Shared;
using ProcureHub.BlazorApp.Tests.Infrastructure;

namespace ProcureHub.BlazorApp.Tests.Features.Shared;

public class AppErrorBoundaryTests : BlazorTestContext
{
    [Fact]
    public void Redirects_to_login_for_unauthenticated_exception()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        var returnUrl = nav.Uri;

        Render<AppErrorBoundary>(parameters =>
            parameters.AddChildContent<ThrowUnauthenticatedExceptionComponent>());

        Assert.StartsWith("http://localhost/Account/Login?returnUrl=", nav.Uri, StringComparison.Ordinal);
        Assert.Contains(Uri.EscapeDataString(returnUrl), nav.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void Redirects_to_access_denied_for_forbidden_exception()
    {
        var nav = Services.GetRequiredService<NavigationManager>();

        Render<AppErrorBoundary>(parameters =>
            parameters.AddChildContent<ThrowForbiddenExceptionComponent>());

        Assert.Equal("http://localhost/Account/AccessDenied", nav.Uri);
    }

    private sealed class ThrowUnauthenticatedExceptionComponent : ComponentBase
    {
        protected override void OnInitialized()
        {
            throw new RequestUnauthenticatedException("test-request");
        }
    }

    private sealed class ThrowForbiddenExceptionComponent : ComponentBase
    {
        protected override void OnInitialized()
        {
            throw new RequestForbiddenException("test-request", ["Admin"]);
        }
    }
}
