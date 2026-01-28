# Blazor Radzen Migration Plan

Mirror the React app (ProcureHub.WebApp) in Blazor Server using Radzen components in the ProcureHub.BlazorApp project.

## Phase 1: Foundation (Layout + Radzen Setup)

### Step 1.1: Install Radzen.Blazor

1. Add NuGet package:
   ```bash
   cd ProcureHub.BlazorApp
   dotnet add package Radzen.Blazor
   ```

2. Register services in `Program.cs`:
   ```csharp
   builder.Services.AddRadzenComponents();
   ```

3. Add CSS/JS to `Components/App.razor` in `<head>`:
   ```html
   <link rel="stylesheet" href="_content/Radzen.Blazor/css/humanistic-base.css">
   <script src="_content/Radzen.Blazor/Radzen.Blazor.js"></script>
   ```

4. Update `Components/_Imports.razor`:
   ```razor
   @using Radzen
   @using Radzen.Blazor
   ```

### Step 1.2: Register Domain Services

1. In `Program.cs`, add reference and call:
   ```csharp
   using ProcureHub.Infrastructure;
   
   builder.Services.AddDomainServices();
   ```

2. This registers all `IRequestHandler` implementations with validation decorators.

### Step 1.3: Create Blazor ICurrentUser Implementation

Handlers use `ICurrentUser` which reads from HttpContext in WebApi. Blazor Server uses `AuthenticationStateProvider` instead.

1. Create `ProcureHub.BlazorApp/Infrastructure/Authentication/CurrentUserFromAuthenticationState.cs`:
   - Inject `AuthenticationStateProvider`
   - Implement `ICurrentUser` interface
   - Extract UserId from `ClaimTypes.NameIdentifier`
   - Extract Roles from `ClaimTypes.Role`

2. Register in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<ICurrentUser, CurrentUserFromAuthenticationState>();
   ```

### Step 1.4: Create Authenticated Layout

Create `Components/Layout/AuthenticatedLayout.razor` using Radzen layout components.

Structure:
```
RadzenLayout
├── RadzenHeader (fixed)
│   ├── RadzenStack (horizontal)
│   │   ├── Sidebar toggle button
│   │   ├── Breadcrumbs (RadzenBreadcrumb)
│   │   └── User menu (RadzenProfileMenu or custom dropdown)
├── RadzenSidebar (collapsible)
│   ├── App logo/title
│   ├── RadzenPanelMenu with nav items
│   │   ├── Overview section
│   │   │   └── Dashboard (/)
│   │   ├── Request Management (AuthorizeView Roles="Requester")
│   │   │   ├── Requests (/requests)
│   │   │   └── New Request (/requests/new)
│   │   ├── Approvals (AuthorizeView Roles="Approver")
│   │   │   └── Pending Approvals (/approvals)
│   │   └── Administration (AuthorizeView Roles="Admin")
│   │       ├── Users (/admin/users)
│   │       └── Departments (/admin/departments)
└── RadzenBody
    └── @Body
```

Reference nav structure from: `ProcureHub.WebApp/src/components/layout/nav-data.ts`

### Step 1.5: Create Unauthenticated Layout

Create `Components/Layout/UnauthenticatedLayout.razor` for Login/Register pages.

Structure:
```
RadzenLayout (centered, minimal)
├── RadzenCard (centered container)
│   ├── App logo
│   └── @Body (login/register form)
```

### Step 1.6: Update Identity Pages to Use Radzen

Convert key Identity pages to Radzen components:

**Login.razor** (`Components/Account/Pages/Login.razor`):
- Add `@layout UnauthenticatedLayout`
- Replace Bootstrap form with:
  - `RadzenTextBox` for email
  - `RadzenPassword` for password
  - `RadzenCheckBox` for "Remember me"
  - `RadzenButton` for submit
  - `RadzenAlert` for validation errors

**Register.razor** (`Components/Account/Pages/Register.razor`):
- Same pattern as Login

**Manage/Index.razor** (profile page):
- Use `@layout AuthenticatedLayout`
- Convert form fields to Radzen components

### Step 1.7: Update Sample Pages

**Home.razor** (`Components/Pages/Home.razor`):
- Add `@layout AuthenticatedLayout`
- Add `@attribute [Authorize]`
- Update content to be a dashboard placeholder

**Counter.razor** and **Weather.razor**:
- Add `@layout AuthenticatedLayout`
- Add `@attribute [Authorize]`
- Convert to Radzen components (`RadzenButton`, `RadzenDataGrid`)

### Step 1.8: Update Routes and Default Layout

In `Components/Routes.razor`:
- Keep `AuthorizeRouteView` (already present)
- Ensure `DefaultLayout` points to `AuthenticatedLayout`

---

## Phase 2: Users Feature

### Step 2.1: Create Users Page Structure

Create folder: `Components/Pages/Admin/Users/`

Files to create:
- `Index.razor` - Main users list page
- `UserDialog.razor` - Create/Edit user dialog
- `ManageRolesDialog.razor` - Assign roles dialog
- `AssignDepartmentDialog.razor` - Assign department dialog

### Step 2.2: Users List Page (Index.razor)

Route: `/admin/users`

Components:
- `RadzenTextBox` for search (with debounce)
- `RadzenButton` for "Add User"
- `RadzenDataGrid` with columns:
  - Avatar + Name + Email (combined cell)
  - Roles (badges via `RadzenBadge`)
  - Department
  - Status (enabled/disabled badge)
  - Actions dropdown (`RadzenSplitButton` or context menu)
- `RadzenPager` for pagination

Inject:
```csharp
@inject IRequestHandler<QueryUsers.Request, PagedResult<QueryUsers.Response>> QueryUsersHandler
```

### Step 2.3: Create/Edit User Dialog

`UserDialog.razor` component:
- `RadzenDialog` wrapper
- `RadzenTemplateForm` with:
  - `RadzenTextBox` for Email, FirstName, LastName
  - `RadzenPassword` for Password (create only)
- Inject `CreateUser` and `UpdateUser` handlers
- Handle validation errors from `Result<T>`

### Step 2.4: Manage Roles Dialog

`ManageRolesDialog.razor`:
- `RadzenCheckBoxList` with available roles
- Inject `QueryRoles` handler to fetch roles
- Inject `AssignRolesToUser` handler to save

### Step 2.5: Assign Department Dialog
 
`AssignDepartmentDialog.razor`:
- `RadzenDropDown` with departments
- Inject `QueryDepartments` handler
- Inject `AssignUserToDepartment` handler

### Step 2.6: Enable/Disable User Actions

In Users table actions:
- Call `EnableUser` / `DisableUser` handlers directly
- Show confirmation via `RadzenDialog.Confirm()`
- Refresh grid after action

---

## Phase 3: Departments + Other Features

Follow same patterns established in Phase 2.

### Departments Feature
- `/admin/departments` route
- CRUD via `RadzenDataGrid` + dialogs
- Handlers: `QueryDepartments`, `CreateDepartment`, `UpdateDepartment`, `DeleteDepartment`

### Categories Feature
- Similar pattern to Departments

### Purchase Requests Feature
- `/requests` - List with status filtering
- `/requests/new` - Create form
- `/requests/{id}` - Details view
- Approval workflow for Approver role

---

## Key Patterns

### Injecting Handlers
```csharp
@inject IRequestHandler<QueryUsers.Request, PagedResult<QueryUsers.Response>> QueryUsersHandler

@code {
    private async Task LoadUsers()
    {
        var result = await QueryUsersHandler.HandleAsync(new QueryUsers.Request { Page = 1, PageSize = 10 }, default);
        // use result.Items, result.TotalCount
    }
}
```

### Handling Results
```csharp
var result = await CreateUserHandler.HandleAsync(request, default);
result.Match(
    userId => { /* success - close dialog, refresh grid */ },
    error => { /* show error message */ }
);
```

### Role-Based UI
```razor
<AuthorizeView Roles="Admin">
    <Authorized>
        <RadzenButton Text="Admin Action" />
    </Authorized>
</AuthorizeView>
```

### Dialog Pattern
```csharp
@inject DialogService DialogService

async Task OpenUserDialog(UserDto? user = null)
{
    var result = await DialogService.OpenAsync<UserDialog>(
        user is null ? "Create User" : "Edit User",
        new Dictionary<string, object> { { "User", user } }
    );
    if (result == true) await LoadUsers();
}
```

---

## Files Summary

### Phase 1 New Files
| File | Purpose |
|------|---------|
| `Infrastructure/Authentication/CurrentUserFromAuthenticationState.cs` | Blazor ICurrentUser impl |
| `Components/Layout/AuthenticatedLayout.razor` | Main app layout with sidebar |
| `Components/Layout/UnauthenticatedLayout.razor` | Login/Register layout |

### Phase 1 Modified Files
| File | Changes |
|------|---------|
| `Program.cs` | Add Radzen, AddDomainServices, ICurrentUser registration |
| `Components/App.razor` | Add Radzen CSS/JS |
| `Components/_Imports.razor` | Add Radzen usings |
| `Components/Account/Pages/Login.razor` | Radzen components + layout |
| `Components/Account/Pages/Register.razor` | Radzen components + layout |
| `Components/Pages/Home.razor` | Auth layout + Authorize attribute |
| `Components/Pages/Counter.razor` | Auth layout + Radzen button |
| `Components/Pages/Weather.razor` | Auth layout + RadzenDataGrid |

### Phase 2 New Files
| File | Purpose |
|------|---------|
| `Components/Pages/Admin/Users/Index.razor` | Users list page |
| `Components/Pages/Admin/Users/UserDialog.razor` | Create/Edit dialog |
| `Components/Pages/Admin/Users/ManageRolesDialog.razor` | Role assignment |
| `Components/Pages/Admin/Users/AssignDepartmentDialog.razor` | Dept assignment |
