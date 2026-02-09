using Microsoft.Playwright;

namespace ProcureHub.BlazorApp.E2ETests.Infrastructure;

public static class BlazorPageExtensions
{
    /// <summary>
    /// Navigates to a Blazor Server page and waits for network idle,
    /// ensuring the Blazor circuit is fully established.
    /// </summary>
    public static Task<IResponse?> GotoBlazorServerPageAsync(this IPage page, string url)
        => page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
}
