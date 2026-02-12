using Microsoft.Playwright;
using ProcureHub.BlazorApp.E2ETests.Infrastructure;

namespace ProcureHub.BlazorApp.E2ETests.Features;

[Collection(BlazorE2ETestCollection.Name)]
public class PurchaseRequestListTests : BlazorPageTest
{
    [Fact]
    public async Task Requester_can_see_own_request_in_grid()
    {
        // Login as requester
        await LoginAsRequesterAsync();

        // Navigate to requests page
        await Page.GotoAsync("/requests");

        // Verify page loaded with heading
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Requests" })).ToBeVisibleAsync();

        // Wait for grid to load - check for a column header first
        await Expect(Page.GetByText("Request #")).ToBeVisibleAsync();

        // The requester should see their pre-seeded requests
        await Expect(Page.GetByText("PR-2025-001")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-002")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-003")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-004")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Search_filters_requests_by_title()
    {
        // Login as requester
        await LoginAsRequesterAsync();

        // Navigate to requests page
        await Page.GotoAsync("/requests");

        // Wait for grid and initial data to load
        await Expect(Page.GetByRole(AriaRole.Grid, new() { Name = "Purchase Requests" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-001")).ToBeVisibleAsync();

        // Type "Laptops" in search box
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Search" }).FillAsync("Laptops");

        // Click Apply button
        await Page.GetByRole(AriaRole.Button, new() { Name = "Apply" }).ClickAsync();

        // Should only show PR-2025-001 ("New Development Laptops")
        await Expect(Page.GetByText("PR-2025-001")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-002")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-003")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-004")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task Clear_filters_shows_all_requests()
    {
        // Login as requester
        await LoginAsRequesterAsync();

        // Navigate to requests page
        await Page.GotoAsync("/requests");

        // Wait for grid to load
        await Expect(Page.GetByRole(AriaRole.Grid, new() { Name = "Purchase Requests" })).ToBeVisibleAsync();

        // Verify all 4 requests are initially visible
        await Expect(Page.GetByText("PR-2025-001")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-002")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-003")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-004")).ToBeVisibleAsync();

        // Type "Laptops" in search box to filter
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Search" }).FillAsync("Laptops");

        // Click Apply button
        await Page.GetByRole(AriaRole.Button, new() { Name = "Apply" }).ClickAsync();

        // Should only show PR-2025-001
        await Expect(Page.GetByText("PR-2025-001")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-002")).Not.ToBeVisibleAsync();

        // Click Clear button
        await Page.GetByRole(AriaRole.Button, new() { Name = "Clear" }).ClickAsync();

        // All requests should be visible again
        await Expect(Page.GetByText("PR-2025-001")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-002")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-003")).ToBeVisibleAsync();
        await Expect(Page.GetByText("PR-2025-004")).ToBeVisibleAsync();
    }
}
