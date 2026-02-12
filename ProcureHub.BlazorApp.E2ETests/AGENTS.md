# Blazor E2E Tests

Playwright-based E2E tests for `ProcureHub.BlazorApp` (Blazor Server). Uses xUnit v3, `Microsoft.Playwright.Xunit.v3`, and `WebApplicationFactory` with Kestrel.

## Project Structure

```
/Infrastructure
  BlazorApplicationFactory.cs  — WAF with DummyHost pattern; starts real Kestrel on random HTTPS port
  BlazorPageTest.cs            — Base test class (extends BrowserTest); DB reset/seed, login helpers, shared constants
  BlazorPageExtensions.cs      — GotoBlazorServerPageAsync() extension (waits for NetworkIdle)
  BlazorE2ETestCollection.cs   — xUnit [CollectionDefinition] for serializing all E2E tests
  DatabaseResetter.cs          — Respawn + DataSeeder, serialized via SemaphoreSlim
  Configuration.cs             — Connection string helper
/Features
  LoginTests.cs                — Login/auth flow tests
  NavigationTests.cs           — Sidebar visibility per role, nav links, profile menu
  AccessControlTests.cs        — Role-gated page access (redirects to AccessDenied)
  UserManagementTests.cs       — Admin user CRUD via RadzenDataGrid
```

## Test Serialization (Important)

All test classes **must** have the `[Collection(BlazorE2ETestCollection.Name)]` attribute. This ensures xUnit runs E2E tests sequentially (not in parallel), preventing DB races where parallel tests trample each other's seeded data. This matches the `[Collection("ApiTestHost")]` pattern used in `ProcureHub.WebApi.Tests`.

## Test Lifecycle

Each test:
1. Spins up a `BlazorApplicationFactory` (Kestrel on random HTTPS port with test DB)
2. Resets DB via Respawn and re-seeds all test data (roles, departments, users, categories, purchase requests)
3. Creates a Playwright browser context + page with `BaseURL` pointed at Kestrel
4. Test runs using `Page`, `LoginAsync()`, `Expect()`, `GotoBlazorServerPageAsync()`
5. Tears down page, context, factory

## Writing Tests

- Extend `BlazorPageTest` (defined in `Infrastructure/BlazorPageTest.cs`)
- Use `LoginAsAdminAsync()`, `LoginAsRequesterAsync()`, `LoginAsApproverAsync()` helpers
- The app uses Radzen UI components — selectors may need Radzen-specific CSS classes (e.g. `.rz-sidebar`, `.rz-profile-menu`)
- If it's particularly difficult to get a test to pass, consider using MCP tools like Chrome DevTools to inspect the dom structure or app behaviour in the real app.

## Test Users

| Email | Password | Roles |
|---|---|---|
| `test-admin@example.com` | `Password1!` | Admin, Requester, Approver |
| `test-requester@example.com` | `Password1!` | Requester |
| `test-approver@example.com` | `Password1!` | Approver |

## Best Practices

### Avoid hard-coded timeouts

IMPORTANT: DO NOT use `Page.WaitForTimeoutAsync` to wait for elements or page state. This approach is brittle (arbitrary wait times) and slows down tests unnecessarily. Playwright includes auto-retrying assertions (such as `ToBeVisibleAsync()`) that remove flakiness by waiting until the condition is met - prefer to use those. Examples:

- Use `await Expect(locator).ToBeVisibleAsync()` or `await Expect(locator).ToBeHiddenAsync()`
- Use `await Page.WaitForURLAsync()` for navigation

Playwright also performs a range of actionability checks on the elements before making actions (such as `Locator.ClickAsync()`) to ensure these actions behave as expected.

For more on Playwright actionability checks and auto-retrying assertions, see: https://playwright.dev/dotnet/docs/actionability.

### Prefer semantic selectors with GetByRole

Always prefer `GetByRole` to locate elements using standard ARIA attributes. This makes tests more resilient to DOM changes and enforces accessibility best practices.

```csharp
// Good
await Expect(sidebar.GetByRole(AriaRole.Link, new() { Name = "Users" })).ToBeVisibleAsync();

// Avoid
await Expect(sidebar.GetByText("Users")).ToBeVisibleAsync();
```

If an element in the source code lacks an appropriate ARIA attribute that would enable `GetByRole` selection, update the source file to add the missing attribute (e.g., `aria-label`, `role`, etc.).

### Verify page structure with ARIA snapshots

For validating overall page structure or complex component hierarchies, use `ToMatchAriaSnapshotAsync`. This creates a readable, maintainable snapshot of the accessible tree.

```csharp
await page.GotoAsync("https://example.com/");
await Expect(page.Locator("banner")).ToMatchAriaSnapshotAsync(@"
  - banner:
    - heading \"Welcome to ProcureHub\" [level=1]
    - link \"Get Started\"
    - link \"Documentation\"
");
```

ARIA snapshots are particularly useful for verifying navigation menus, form structures, and page layouts without relying on fragile CSS selectors.
