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
- Use `Page.GotoBlazorServerPageAsync("/route")` instead of raw `GotoAsync` (waits for Blazor circuit)
- Use `DefaultNavigationTimeoutMs` const for all `WaitForURLAsync` timeouts
- The app uses Radzen UI components — selectors may need Radzen-specific CSS classes (e.g. `.rz-sidebar`, `.rz-profile-menu`)

## Test Users

| Email | Password | Roles |
|---|---|---|
| `test-admin@example.com` | `Password1!` | Admin, Requester, Approver |
| `test-requester@example.com` | `Password1!` | Requester |
| `test-approver@example.com` | `Password1!` | Approver |
