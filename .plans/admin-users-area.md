# Plan: Implement Admin Users Area (Manage Users, Assign Roles, Configure Departments)

This plan covers implementing three admin use cases: user account management (create/deactivate), role assignment (approval/purchasing/admin permissions), and department configuration. The implementation follows Vertical Slice Architecture (VSA) and requires both API endpoints and React UI components.

## Steps

1. **Add User Management API Endpoints** - In [ProcureHub.WebApi/Features/Users](ProcureHub.WebApi/Features/Users), add command-focused endpoints: `PUT /users/{id}` (update basic profile: firstName, lastName, email), `PATCH /users/{id}/enable` (enable user account), `PATCH /users/{id}/disable` (disable user account), `PATCH /users/{id}/department` (assign user to department). Update existing GET endpoints to include user roles in responses. Add tests in [ProcureHub.WebApi.Tests/Features](ProcureHub.WebApi.Tests/Features).

2. **Add Role Management API Endpoints** - Create [ProcureHub.WebApi/Features/Roles](ProcureHub.WebApi/Features/Roles) feature folder with endpoints: `POST /users/{id}/roles` (assign role), `DELETE /users/{id}/roles/{roleId}` (remove role), and `GET /roles` (list available roles). Include corresponding request handlers in [ProcureHub/Features/Roles](ProcureHub/Features/Roles) and tests.

3. **Add Department Management API Endpoints** - In [ProcureHub.WebApi/Features/Departments](ProcureHub.WebApi/Features/Departments), add `PUT /departments/{id}` (update department name) and `DELETE /departments/{id}` (block deletion if department has active users, return validation error with user count). Update authorization to require `AdminOnly` policy for create/update/delete operations. Add tests.

4. **Update OpenAPI Client and Build Admin UI Routes** - Run `npm run generate:api-schema` in [ProcureHub.WebApp](ProcureHub.WebApp), then create [src/routes/\_app-layout/admin/users/index.tsx](ProcureHub.WebApp/src/routes/_app-layout/admin/users/index.tsx) (user list with search/filter) and [src/routes/\_app-layout/admin/departments/index.tsx](ProcureHub.WebApp/src/routes/_app-layout/admin/departments/index.tsx) (department list).

5. **Implement User Management UI** - Create [src/features/users](ProcureHub.WebApp/src/features/users) with components for user table, create/edit dialog, role assignment multi-select, and enable/disable toggle. Include form validation using Zod schemas and TanStack Query mutations.

6. **Implement Department Management UI** - Create [src/features/departments](ProcureHub.WebApp/src/features/departments) with department table, create/edit dialog, and delete confirmation with user reassignment warning. Use shadcn/ui components (`Table`, `Dialog`, `Form`).

## Design Decisions

1. **Enable/Disable vs Soft Delete** - Use `EnabledAt` field for account status (enable/disable toggle). Reserve `DeletedAt` field for soft delete audit trail (future use).

2. **Department Deletion with Active Users** - Block deletion and require manual user reassignment first. Return validation error indicating number of active users in department.

3. **Password Management** - Out of scope for this iteration. No password reset endpoints.

4. **Budget Tracking** - Out of scope for this iteration. Department management focuses on name and organizational structure only.

5. **Command-Focused Endpoints** - Use slim update endpoints for basic profile edits (user name, department name). Use dedicated command endpoints for actions (enable/disable user, assign department, assign roles).
