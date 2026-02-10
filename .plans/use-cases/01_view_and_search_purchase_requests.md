# View My Requests & Search/Filter Requests

## Use Cases

- **View My Requests** — User sees all their submitted requests and their status
- **Search/Filter Requests** — Find requests by status, department, or search text (date range and amount filters deferred)

## Current State

The `QueryPurchaseRequests` handler already supports:
- Role-based visibility (admin sees all, approver sees dept + own, requester sees own)
- Filter by `Status`
- Search by `Title` / `RequestNumber`
- Pagination

The API endpoint `GET /purchase-requests` exposes `status`, `search`, `page`, `pageSize` query params.

The Blazor `Index.razor` page at `/requests` is a stub with a TODO comment.

## Changes Needed

### 0. Fix DataSeeder — remove hardcoded emails in SeedPurchaseRequestsAsync

`SeedPurchaseRequestsAsync()` hardcodes `requester@example.com` and `approver@example.com`. This breaks seeding in environments with different emails (e.g. E2E tests use `test-requester@example.com`).

Fix: find the requester and approver users from the `SeedUsers` config section by checking if their email contains "requester" or "approver", respectively. This makes purchase request seeding work across all environments (dev, Blazor, E2E).

### 1. Backend — Add department filter to QueryPurchaseRequests

Add a `DepartmentId` (Guid?) parameter to `QueryPurchaseRequests.Request`. When provided, filter results to only that department. Update the API endpoint to accept `departmentId` query param.

### 2. Backend — Add API test for department filter

Add a test in `PurchaseRequestTests` that creates requests in two departments, queries with `departmentId` filter, and asserts only matching requests are returned.

### 3. Blazor UI — Implement requests listing page

Replace the `/requests` stub with a full page using `RadzenDataGrid` (server-side paging via `LoadData`). Pattern follows `Admin/Users/Index.razor`.

Features:
- Search bar (text search by title/request number)
- Status dropdown filter (all statuses + "All")
- Department dropdown filter (all departments user can see + "All")
- Data grid columns: Request #, Title, Department, Status, Estimated Amount, Requester, Created
- Status rendered as colored `RadzenBadge`
- Click row to navigate to request detail (future — for now, no action)

### 4. No new endpoint needed

The existing `GET /purchase-requests` endpoint is sufficient. The Blazor app calls handlers directly anyway.

## Architecture Notes

- Blazor app calls `QueryPurchaseRequests.Handler` and `QueryDepartments.Handler` directly (no HTTP)
- `ICurrentUserProvider` provides auth context for the query handler
- Sequential handler calls (no parallel DbContext usage)

## Tests

### API test
- `Can_filter_purchase_requests_by_department` in `PurchaseRequestTests`

### Blazor E2E smoke test
- `PurchaseRequestListTests` in `ProcureHub.BlazorApp.E2ETests/Features/`
- Test: `Requester_can_see_own_request_in_grid`
  - Login as requester, navigate to `/requests`, verify the grid loads and seeded purchase requests appear with correct data
  - Purchase requests are pre-seeded by DataSeeder (after the fix in step 0), so no need to create data via UI
