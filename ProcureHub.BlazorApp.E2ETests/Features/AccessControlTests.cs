using Microsoft.Playwright;
using ProcureHub.BlazorApp.E2ETests.Infrastructure;

namespace ProcureHub.BlazorApp.E2ETests.Features;

[Collection(BlazorE2ETestCollection.Name)]
public class AccessControlTests : BlazorPageTest
{
    [Fact]
    public async Task Requester_cannot_access_admin_users_page()
    {
        await LoginAsRequesterAsync();

        await Page.GotoAsync("/admin/users");

        await Page.WaitForURLAsync(url => url.Contains("/Account/AccessDenied"), new()
        {
            Timeout = DefaultNavigationTimeoutMs
        });
    }

    [Fact]
    public async Task Requester_cannot_access_admin_departments_page()
    {
        await LoginAsRequesterAsync();

        await Page.GotoAsync("/admin/departments");

        await Page.WaitForURLAsync(url => url.Contains("/Account/AccessDenied"), new()
        {
            Timeout = DefaultNavigationTimeoutMs
        });
    }

    [Fact]
    public async Task Approver_cannot_access_admin_users_page()
    {
        await LoginAsApproverAsync();

        await Page.GotoAsync("/admin/users");

        await Page.WaitForURLAsync(url => url.Contains("/Account/AccessDenied"), new()
        {
            Timeout = DefaultNavigationTimeoutMs
        });
    }

    [Fact]
    public async Task Approver_cannot_access_new_request_page()
    {
        await LoginAsApproverAsync();

        await Page.GotoAsync("/requests/new");

        await Page.WaitForURLAsync(url => url.Contains("/Account/AccessDenied"), new()
        {
            Timeout = DefaultNavigationTimeoutMs
        });
    }

    [Fact]
    public async Task Requester_cannot_access_approvals_page()
    {
        await LoginAsRequesterAsync();

        await Page.GotoAsync("/approvals");

        await Page.WaitForURLAsync(url => url.Contains("/Account/AccessDenied"), new()
        {
            Timeout = DefaultNavigationTimeoutMs
        });
    }

    [Fact]
    public async Task Admin_can_access_admin_users_page()
    {
        await LoginAsAdminAsync();

        await Page.GotoAsync("/admin/users");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Users" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Admin_can_access_admin_departments_page()
    {
        await LoginAsAdminAsync();

        await Page.GotoAsync("/admin/departments");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Departments" })).ToBeVisibleAsync();
    }
}
