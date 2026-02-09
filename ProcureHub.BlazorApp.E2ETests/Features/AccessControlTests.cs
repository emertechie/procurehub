using Microsoft.Playwright;
using ProcureHub.BlazorApp.E2ETests.Infrastructure;

namespace ProcureHub.BlazorApp.E2ETests.Features;

public class AccessControlTests : BlazorPageTest<Program>
{
    [Fact]
    public async Task Requester_cannot_access_admin_users_page()
    {
        await LoginAsRequesterAsync();

        await Page.GotoBlazorServerPageAsync("/admin/users");

        await Page.WaitForURLAsync(url => url.Contains("/Account/AccessDenied"), new()
        {
            Timeout = 10000
        });
    }

    [Fact]
    public async Task Requester_cannot_access_admin_departments_page()
    {
        await LoginAsRequesterAsync();

        await Page.GotoBlazorServerPageAsync("/admin/departments");

        await Page.WaitForURLAsync(url => url.Contains("/Account/AccessDenied"), new()
        {
            Timeout = 10000
        });
    }

    [Fact]
    public async Task Approver_cannot_access_admin_users_page()
    {
        await LoginAsApproverAsync();

        await Page.GotoBlazorServerPageAsync("/admin/users");

        await Page.WaitForURLAsync(url => url.Contains("/Account/AccessDenied"), new()
        {
            Timeout = 10000
        });
    }

    [Fact]
    public async Task Approver_cannot_access_new_request_page()
    {
        await LoginAsApproverAsync();

        await Page.GotoBlazorServerPageAsync("/requests/new");

        await Page.WaitForURLAsync(url => url.Contains("/Account/AccessDenied"), new()
        {
            Timeout = 10000
        });
    }

    [Fact]
    public async Task Requester_cannot_access_approvals_page()
    {
        await LoginAsRequesterAsync();

        await Page.GotoBlazorServerPageAsync("/approvals");

        await Page.WaitForURLAsync(url => url.Contains("/Account/AccessDenied"), new()
        {
            Timeout = 10000
        });
    }

    [Fact]
    public async Task Admin_can_access_admin_users_page()
    {
        await LoginAsAdminAsync();

        await Page.GotoBlazorServerPageAsync("/admin/users");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Users" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Admin_can_access_admin_departments_page()
    {
        await LoginAsAdminAsync();

        await Page.GotoBlazorServerPageAsync("/admin/departments");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Departments" })).ToBeVisibleAsync();
    }
}
