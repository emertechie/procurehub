using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ProcureHub.Application.Common.Authorization;

namespace ProcureHub.BlazorApp.Components.Shared;

public sealed class AppErrorBoundary : ErrorBoundary
{
    private const string AccessDeniedPath = "/Account/AccessDenied";

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ILogger<AppErrorBoundary> Logger { get; set; } = null!;

    protected override Task OnErrorAsync(Exception exception)
    {
        var authException = FindAuthException(exception);

        switch (authException)
        {
            case RequestUnauthenticatedException:
                RedirectToLogin();
                break;

            case RequestForbiddenException:
                NavigationManager.NavigateTo(AccessDeniedPath);
                break;

            default:
                Logger.LogError(exception, "Unhandled UI exception");
                break;
        }

        return Task.CompletedTask;
    }

    private void RedirectToLogin()
    {
        var returnUrl = Uri.EscapeDataString(NavigationManager.Uri);
        NavigationManager.NavigateTo($"Account/Login?returnUrl={returnUrl}", forceLoad: true);
    }

    private static Exception? FindAuthException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is RequestUnauthenticatedException || current is RequestForbiddenException)
            {
                return current;
            }
        }

        return null;
    }
}
