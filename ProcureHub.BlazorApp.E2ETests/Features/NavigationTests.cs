using Microsoft.Playwright;
using ProcureHub.BlazorApp.E2ETests.Infrastructure;

namespace ProcureHub.BlazorApp.E2ETests.Features;

[Collection(BlazorE2ETestCollection.Name)]
public class NavigationTests : BlazorPageTest
{
    [Fact]
    public async Task Admin_sees_all_sidebar_sections()
    {
        await LoginAsAdminAsync();

        var sidebar = Page.Locator(".rz-sidebar");

        await Expect(sidebar.GetByText("Home")).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Requests")).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("New Request")).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Pending Approvals")).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Users")).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Departments")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Requester_sees_request_items_but_not_admin_or_approvals()
    {
        await LoginAsRequesterAsync();

        var sidebar = Page.Locator(".rz-sidebar");

        await Expect(sidebar.GetByText("Home")).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Requests")).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("New Request")).ToBeVisibleAsync();

        await Expect(sidebar.GetByText("Pending Approvals")).Not.ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Users")).Not.ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Departments")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task Approver_sees_approvals_but_not_admin_or_new_request()
    {
        await LoginAsApproverAsync();

        var sidebar = Page.Locator(".rz-sidebar");

        await Expect(sidebar.GetByText("Home")).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Requests")).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Pending Approvals")).ToBeVisibleAsync();

        await Expect(sidebar.GetByText("New Request")).Not.ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Users")).Not.ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Departments")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task Clicking_sidebar_users_navigates_to_users_page()
    {
        await LoginAsAdminAsync();

        await Page.Locator(".rz-sidebar").GetByText("Users").ClickAsync();

        await Page.WaitForURLAsync(url => url.Contains("/admin/users"), new()
        {
            Timeout = DefaultNavigationTimeoutMs
        });
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Users" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Clicking_sidebar_departments_navigates_to_departments_page()
    {
        await LoginAsAdminAsync();

        await Page.Locator(".rz-sidebar").GetByText("Departments").ClickAsync();

        await Page.WaitForURLAsync(url => url.Contains("/admin/departments"), new()
        {
            Timeout = DefaultNavigationTimeoutMs
        });
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Departments" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Profile_menu_shows_profile_and_logout_options()
    {
        await LoginAsAdminAsync();

        // Open the profile menu
        await Page.Locator(".rz-profile-menu").ClickAsync();

        await Expect(Page.GetByText("Profile")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Logout")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Logout_redirects_to_login_page()
    {
        await LoginAsAdminAsync();

        // Open profile menu and click logout
        await Page.Locator(".rz-profile-menu").ClickAsync();
        await Page.GetByText("Logout").ClickAsync();

        await Page.WaitForURLAsync(url => url.Contains("/Account/Login"), new()
        {
            Timeout = DefaultNavigationTimeoutMs
        });
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Log in" })).ToBeVisibleAsync();
    }
}
