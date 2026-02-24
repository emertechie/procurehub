using Microsoft.Playwright;
using ProcureHub.BlazorApp.E2ETests.Infrastructure;

namespace ProcureHub.BlazorApp.E2ETests.Features;

[Collection(BlazorE2ETestCollection.Name)]
public class UserManagementTests : BlazorPageTest
{
    private async Task NavigateToUsersPage()
    {
        await LoginAsAdminAsync();
        await Page.GotoAsync("/admin/users");
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

        // Wait for grid and data to load
        await Expect(Page.GetByRole(AriaRole.Grid, new() { Name = "Users" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("test-requester@example.com")).ToBeVisibleAsync();

        // Search for requester — press Tab after fill to blur the input and trigger Change
        var searchBox = Page.GetByPlaceholder("Search users...");
        await searchBox.FillAsync("requester");
        await searchBox.PressAsync("Tab");
        
        // Wait for grid to update by asserting filtered-out rows disappear (Playwright auto-retries)
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

        // Wait for dialog to close and new user to appear in grid
        await Expect(Page.GetByText("newuser@example.com")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Edit_user_via_dialog()
    {
        await NavigateToUsersPage();

        // Find the requester row and click edit
        var row = Page.Locator("tr", new() { HasText = "test-requester@example.com" });
        await row.GetByRole(AriaRole.Button, new() { Name = "Edit user profile" }).ClickAsync();

        // Wait for edit dialog
        await Expect(Page.GetByText("Edit User").First).ToBeVisibleAsync();

        // Change first name
        var firstNameInput = Page.Locator("[name='FirstNameInput']");
        await firstNameInput.ClearAsync();
        await firstNameInput.FillAsync("UpdatedName");

        // Save
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // Wait for dialog to close and updated name to appear
        await Expect(row.GetByText("UpdatedName")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Disable_and_enable_user()
    {
        await NavigateToUsersPage();

        // Find the requester row — should be Enabled initially
        var requesterRow = Page.Locator("tr", new() { HasText = "test-requester@example.com" });
        await Expect(requesterRow.GetByText("Enabled")).ToBeVisibleAsync();

        // Click disable button (block icon)
        await requesterRow.GetByRole(AriaRole.Button, new() { Name = "Disable user" }).ClickAsync();

        // Confirm dialog
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();

        // Wait for grid to reflect Disabled status
        await Expect(requesterRow.GetByText("Disabled")).ToBeVisibleAsync();

        // Re-enable: click enable button (check_circle icon)
        await requesterRow.GetByRole(AriaRole.Button, new() { Name = "Enable user" }).ClickAsync();

        // Confirm
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();

        // Wait for grid to reflect Enabled status
        await Expect(requesterRow.GetByText("Enabled")).ToBeVisibleAsync();
    }

    /* TODO: update useless tests below
    [Fact]
    public async Task Manage_roles_dialog_shows_checkboxes()
    {
        await NavigateToUsersPage();

        // Find the requester row and click manage roles
        var requesterRow = Page.Locator("tr", new() { HasText = "test-requester@example.com" });
        await requesterRow.GetByRole(AriaRole.Button, new() { Name = "Manage roles" }).ClickAsync();
        
        // Wait for dialog
        // TODO: get by role?
        await Expect(Page.GetByText("Manage Roles").First).ToBeVisibleAsync();

        // Should show role checkboxes
        // TODO: get by role?
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
    */
}
