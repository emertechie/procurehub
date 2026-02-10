# Blazor App Testing Plan

Add two test projects for the BlazorApp: bUnit component tests and Playwright E2E tests.

## Project Structure

```
ProcureHub.BlazorApp.Tests/          # bUnit component tests (xUnit v3)
ProcureHub.BlazorApp.E2ETests/       # Playwright E2E tests (xUnit v3)
```

## Project 1: `ProcureHub.BlazorApp.Tests` (bUnit)

Fast, in-process component unit tests. All domain handlers are mocked.

### Packages

- `bunit`
- `xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`
- `NSubstitute` (mock `IQueryHandler`/`ICommandHandler` interfaces)
- `coverlet.collector`
- ProjectRef: `ProcureHub.BlazorApp`

### Infrastructure

1. **`BlazorTestContext`** base class (inherits bUnit `TestContext`):
   - Pre-registers Radzen services (`DialogService`, `NotificationService`, `TooltipService`)
   - Exposes `TestAuthorizationContext` for faking `[Authorize]`/roles
   - Helper to register mock handlers: `AddMockHandler<TReq, TRes>(TRes response)`
2. No DB, no `WebApplicationFactory` — pure in-process rendering

### What to Test

- Components render correct markup given mock handler responses
- Button/action clicks invoke correct handlers with correct args
- Loading, empty, and error states
- Authorization gating (e.g. Users/Index requires `Admin` role)
- Dialog open/close flows via Radzen `DialogService`
- Form validation and submission

### Example Test Pattern

```csharp
public class UsersIndexTests : BlazorTestContext
{
    [Fact]
    public void Renders_User_List()
    {
        // Arrange
        var handler = Substitute.For<IQueryHandler<QueryUsers.Request, PagedResult<QueryUsers.Response>>>();
        handler.HandleAsync(Arg.Any<QueryUsers.Request>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<QueryUsers.Response> { ... });
        Services.AddSingleton(handler);
        AuthContext.SetAuthorized("admin").SetRoles("Admin");

        // Act
        var cut = RenderComponent<Index>();

        // Assert - verify grid rows rendered
    }
}
```

---

## Project 2: `ProcureHub.BlazorApp.E2ETests` (Playwright)

Full browser tests against the real running app with real DB and Identity.

### Packages

- `Microsoft.Playwright.Xunit.v3` (provides `PageTest` base class)
- `Microsoft.AspNetCore.Mvc.Testing`
- `xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`
- `Respawn` (DB reset)
- `Microsoft.EntityFrameworkCore.SqlServer`
- `coverlet.collector`
- ProjectRef: `ProcureHub.BlazorApp`

### Infrastructure

1. **`BlazorAppTestHost`** — extends `WebApplicationFactory<Program>`:
   - References `ProcureHub.BlazorApp.Program`
   - Sets environment to `"Test"`
   - Replaces `ApplicationDbContext` with test DB connection string
   - Runs migrations once (static lock pattern, same as `ApiTestHost`)
   - Separate DB from WebApi tests (e.g. `ProcureHub_BlazorE2ETests`)
2. **Collection fixture** — starts `BlazorAppTestHost` once, exposes base URL
3. **`PlaywrightPageTest`** base class:
   - Inherits `PageTest` from `Microsoft.Playwright.Xunit.v3`
   - Gets app base URL from fixture
   - Overrides `ContextOptions()` to set `BaseURL`
   - Login helper methods (navigate to `/Account/Login`, fill credentials, submit)
4. **`DatabaseResetter`** — Respawn-based, resets between tests
5. **`appsettings.json`** — test-specific config with unique connection string

### Auth Approach

- BlazorApp uses Identity cookie auth (cookie: `.AspNetCore.Identity.BlazorApp`)
- E2E tests log in through the real `/Account/Login` page
- Seed a test admin user via migrations or test setup (same pattern as WebApi tests)

### Example Test

```csharp
public class UserManagementE2ETests : PlaywrightPageTest
{
    [Fact]
    public async Task Admin_Can_Create_User()
    {
        await LoginAsAdmin();
        await Page.GotoAsync("/admin/users");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create User" }).ClickAsync();
        // fill dialog fields, submit, assert user appears in grid
    }
}
```

---

## Solution Changes

- Add both `.csproj` files to `ProcureHub.sln`
- Each project has own `appsettings.json` with unique test DB connection string

## CI

- **bUnit**: `dotnet test ProcureHub.BlazorApp.Tests` — fast, no special deps
- **Playwright**: requires `playwright install chromium` before running
  ```
  dotnet build ProcureHub.BlazorApp.E2ETests
  pwsh ProcureHub.BlazorApp.E2ETests/bin/.../playwright.ps1 install chromium
  dotnet test ProcureHub.BlazorApp.E2ETests
  ```
- Run as separate CI steps (bUnit failures shouldn't block E2E and vice versa)

## Implementation Order

1. bUnit project — lower complexity, faster feedback, more immediately useful
2. Playwright E2E project — more infrastructure (WebApplicationFactory, login helpers, DB reset)
3. Write 1-2 example tests in each to validate setup before expanding coverage
