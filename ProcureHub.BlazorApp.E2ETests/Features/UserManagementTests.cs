using Microsoft.Playwright;
using ProcureHub.BlazorApp.E2ETests.Infrastructure;

namespace ProcureHub.BlazorApp.E2ETests.Features;

public class UserManagementTests : BlazorPageTest<Program>
{
    private async Task NavigateToUsersPage()
    {
        await LoginAsAdminAsync();
        await Page.GotoBlazorServerPageAsync("/admin/users");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Users" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Users_page_shows_seeded_users_in_grid()
    {
        await NavigateToUsersPage();

        // Grid should show the seeded users
        await Expect(Page.GetByText("test-admin@example.com")).ToBeVisibleAsync();
        await Expect(Page.GetByText("test-requester@example.com")).ToBeVisibleAsync();
        await Expect(Page.GetByText("test-approver@example.com")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Search_filters_users_by_email()
    {
        await NavigateToUsersPage();

        // Search for requester
        await Page.GetByPlaceholder("Search users...").FillAsync("requester");
        // Wait for grid to update
        await Page.WaitForTimeoutAsync(500);

        await Expect(Page.GetByText("test-requester@example.com")).ToBeVisibleAsync();
        await Expect(Page.GetByText("test-admin@example.com")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByText("test-approver@example.com")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task Create_user_via_dialog()
    {
        await NavigateToUsersPage();

        // Click "Create User" button
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create User" }).ClickAsync();

        // Wait for dialog to appear
        await Expect(Page.GetByText("Create User").First).ToBeVisibleAsync();

        // Fill form
        await Page.Locator("[name='EmailInput']").FillAsync("newuser@example.com");
        await Page.Locator("[name='FirstNameInput']").FillAsync("New");
        await Page.Locator("[name='LastNameInput']").FillAsync("User");
        await Page.Locator("[name='PasswordInput']").FillAsync("Password1!");

        // Save
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // Wait for dialog to close and grid to refresh
        await Page.WaitForTimeoutAsync(1000);

        // New user should appear in grid
        await Expect(Page.GetByText("newuser@example.com")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Edit_user_via_dialog()
    {
        await NavigateToUsersPage();

        // Find the requester row and click edit
        var requesterRow = Page.Locator("tr", new() { HasText = "test-requester@example.com" });
        await requesterRow.Locator("button[title='Edit']").ClickAsync();

        // Wait for edit dialog
        await Expect(Page.GetByText("Edit User").First).ToBeVisibleAsync();

        // Change first name
        var firstNameInput = Page.Locator("[name='FirstNameInput']");
        await firstNameInput.ClearAsync();
        await firstNameInput.FillAsync("Updated");

        // Save
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // Wait for dialog to close and grid to refresh
        await Page.WaitForTimeoutAsync(1000);

        // Updated name should appear
        await Expect(Page.GetByText("Updated")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Disable_and_enable_user()
    {
        await NavigateToUsersPage();

        // Find the requester row — should be Enabled initially
        var requesterRow = Page.Locator("tr", new() { HasText = "test-requester@example.com" });
        await Expect(requesterRow.GetByText("Enabled")).ToBeVisibleAsync();

        // Click disable button (block icon)
        await requesterRow.Locator("button[title='Disable']").ClickAsync();

        // Confirm dialog
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();

        // Wait for grid refresh
        await Page.WaitForTimeoutAsync(1000);

        // Should now show Disabled
        await Expect(requesterRow.GetByText("Disabled")).ToBeVisibleAsync();

        // Re-enable: click enable button (check_circle icon)
        await requesterRow.Locator("button[title='Enable']").ClickAsync();

        // Confirm
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();

        // Wait for grid refresh
        await Page.WaitForTimeoutAsync(1000);

        // Should be Enabled again
        await Expect(requesterRow.GetByText("Enabled")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Manage_roles_dialog_shows_checkboxes()
    {
        await NavigateToUsersPage();

        // Find the requester row and click manage roles
        var requesterRow = Page.Locator("tr", new() { HasText = "test-requester@example.com" });
        await requesterRow.Locator("button[title='Manage Roles']").ClickAsync();

        // Wait for dialog
        await Expect(Page.GetByText("Manage Roles").First).ToBeVisibleAsync();

        // Should show role checkboxes
        await Expect(Page.GetByText("Admin")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Requester")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Approver")).ToBeVisibleAsync();

        // Cancel
        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
    }

    [Fact]
    public async Task Assign_department_dialog_shows_dropdown()
    {
        await NavigateToUsersPage();

        // Find the requester row and click assign department
        var requesterRow = Page.Locator("tr", new() { HasText = "test-requester@example.com" });
        await requesterRow.Locator("button[title='Assign Department']").ClickAsync();

        // Wait for dialog
        await Expect(Page.GetByText("Assign Department").First).ToBeVisibleAsync();

        // Should show department label
        await Expect(Page.GetByText("Department")).ToBeVisibleAsync();

        // Cancel
        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
    }
}
