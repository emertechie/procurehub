# View My Requests & Search/Filter — Implementation Steps

## Phase 0: Fix DataSeeder

- [x] 0.1 In `SeedPurchaseRequestsAsync`, replace hardcoded `FindByEmailAsync("requester@example.com")` / `FindByEmailAsync("approver@example.com")` with lookup from `SeedUsers` config section (find emails containing "requester" / "approver")
- [x] 0.2 Build & run existing tests to verify no regression

## Phase 1: Backend

- [x] 1.1 Add `DepartmentId` (Guid?) to `QueryPurchaseRequests.Request`, add filter logic in `Handler`, wire through API endpoint
- [x] 1.2 Add `Can_filter_purchase_requests_by_department` test in `PurchaseRequestTests`, add endpoint to `GetAllPurchaseRequestEndpoints`
- [x] 1.3 Build & run tests, fix any failures

## Phase 2: Blazor UI

- [x] 2.1 Implement `/requests` page (`Index.razor`) with `RadzenDataGrid`, search bar, status dropdown, department dropdown, server-side paging via `LoadData`
- [x] 2.2 Build, manually verify page loads (if app is running)

## Phase 3: E2E Test

- [ ] 3.1 Add `PurchaseRequestListTests.cs` — `Requester_can_see_own_request_in_grid` (login, navigate to `/requests`, assert grid shows pre-seeded data)
- [ ] 3.2 Run E2E test, fix any failures
