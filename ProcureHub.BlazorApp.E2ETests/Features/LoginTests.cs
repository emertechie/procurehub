using Microsoft.Playwright;
using ProcureHub.BlazorApp.E2ETests.Infrastructure;

namespace ProcureHub.BlazorApp.E2ETests.Features;

[Collection(BlazorE2ETestCollection.Name)]
public class LoginTests : BlazorPageTest
{
    [Fact]
    public async Task Unauthenticated_user_is_redirected_to_login()
    {
        await Page.GotoAsync("/");

        await Page.WaitForURLAsync(url => url.Contains("/Account/Login"), new()
        {
            Timeout = DefaultNavigationTimeoutMs
        });

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Log in" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Admin_can_login_and_see_dashboard()
    {
        await LoginAsAdminAsync();

        await Expect(Page.GetByText("Dashboard")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Welcome to ProcureHub")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Invalid_login_shows_error()
    {
        await Page.GotoAsync("/Account/Login");
        await Page.FillAsync("[name='Input.Email']", "wrong@example.com");
        await Page.FillAsync("[name='Input.Password']", "WrongPassword1!");
        await Page.ClickAsync("button[type='submit']");

        await Expect(Page.GetByText("Invalid login attempt")).ToBeVisibleAsync();
    }
}
