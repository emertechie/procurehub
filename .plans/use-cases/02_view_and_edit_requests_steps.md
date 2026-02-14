# View/Edit Purchase Requests - Implementation Steps

## Phase 1: Backend Implementation

### Step 1: Add Withdraw method to PurchaseRequest model
- [x] Open `ProcureHub/Models/PurchaseRequest.cs`
- [x] Add `Withdraw()` method after `CanDelete()` method (around line 101)
- [x] Method should:
  - Check if Status is Pending, return failure if not
  - Set Status to Draft
  - Set SubmittedAt to null
  - Set UpdatedAt to DateTime.UtcNow
  - Return Result.Success()

### Step 2: Add CannotWithdrawNonPending error
- [x] Open `ProcureHub/Features/PurchaseRequests/Validation/PurchaseRequestErrors.cs`
- [x] Add `CannotWithdrawNonPending` error after `CannotDeleteNonDraft`
- [x] Use similar pattern: Error.Validation with appropriate title, detail, and Status field error

### Step 3: Create WithdrawPurchaseRequest command handler
- [x] Create new file `ProcureHub/Features/PurchaseRequests/WithdrawPurchaseRequest.cs`
- [x] Add Command record with Id property
- [x] Add CommandValidator with Id.NotEmpty() rule
- [x] Add Handler that:
  - Loads purchase request by Id
  - Returns NotFound if doesn't exist
  - Calls pr.Withdraw()
  - Saves changes if successful
  - Returns result

### Step 4: Add Withdraw endpoint
- [x] Open `ProcureHub.WebApi/Features/PurchaseRequests/Endpoints.cs`
- [x] Add MapPost endpoint for `/purchase-requests/{id:guid}/withdraw` after Delete endpoint
- [x] Follow pattern of other command endpoints
- [x] Require Requester role authorization
- [x] Produces 204 NoContent and 404 NotFound

### Step 5: Add API tests for Withdraw endpoint
- [x] Open `ProcureHub.WebApi.Tests/Features/PurchaseRequestTests.cs`
- [x] Add withdraw endpoint to `GetAllPurchaseRequestEndpoints` theory data
- [x] Add `Test_WithdrawPurchaseRequest_validation` method
- [x] Add `Can_withdraw_pending_purchase_request` test
- [x] Add `Cannot_withdraw_draft_purchase_request` test  
- [x] Add `Cannot_withdraw_approved_purchase_request` test
- [x] Add `Cannot_withdraw_rejected_purchase_request` test

## Phase 2: UI Implementation

### Step 6: Create View/Edit Request page
- [x] Create new file `ProcureHub.BlazorApp/Components/Pages/Requests/View.razor`
- [x] Add `@page "/requests/{Id:guid}"` route
- [x] Add `[Authorize]` attribute
- [x] Inject required handlers:
  - GetPurchaseRequestById handler
  - UpdatePurchaseRequest handler
  - SubmitPurchaseRequest handler
  - WithdrawPurchaseRequest handler
  - DeletePurchaseRequest handler
  - QueryCategories handler
  - QueryDepartments handler
  - CurrentUserProvider
  - NavigationManager
  - NotificationService
  - ILogger

### Step 7: Implement page structure
- [x] Add PageTitle and PageHeader
- [x] Create two-column layout (8/4 ratio like New.razor)
- [x] Left column: Request Details card
- [x] Right column: Actions card

### Step 8: Implement loading state
- [x] Add `_loading` field
- [x] Show RadzenProgressBarCircular while loading
- [x] Load request data in OnInitializedAsync
- [x] Load categories and departments for dropdowns

### Step 9: Implement Draft view (editable)
- [x] When Status == Draft:
  - Show EditForm with FluentValidator
  - Show editable fields: Title, Description, Category, Department, Amount, Justification
  - Actions: "Save Changes", "Submit for Approval", "Delete"

### Step 10: Implement Pending view (readonly with withdraw)
- [x] When Status == Pending:
  - Show readonly display of all fields (RadzenText, not form inputs)
  - Show status badge prominently
  - Show SubmittedAt date
  - Actions: "Withdraw Request" button

### Step 11: Implement Approved/Rejected view (readonly)
- [x] When Status is Approved or Rejected:
  - Show readonly display of all fields
  - Show status badge prominently
  - Show SubmittedAt, ReviewedAt, ReviewedBy info
  - No action buttons

### Step 12: Implement action handlers
- [x] SaveChanges method: calls Update handler
- [x] SubmitRequest method: calls Submit handler, navigates on success
- [x] WithdrawRequest method: calls Withdraw handler, refreshes data on success
- [x] DeleteRequest method: calls Delete handler, navigates on success
- [x] All methods show notifications on success/failure

### Step 13: Update Requests list navigation
- [x] Open `ProcureHub.BlazorApp/Components/Pages/Requests/Index.razor`
- [x] Verify view button navigates to `/requests/{pr.Id}`
- [x] Consider making RequestNumber clickable as well

## Phase 3: Testing and Validation

### Step 14: Build and verify
- [x] Run `dotnet build` to ensure no compilation errors
- [x] Run API tests to verify withdraw functionality
- [x] Verify all tests pass

### Step 15: Manual UI testing
- [ ] Navigate to requests list
- [ ] Click on a Draft request - should show editable form
- [ ] Save changes - should persist
- [ ] Submit - should change to Pending and show readonly view
- [ ] Withdraw - should return to Draft
- [ ] Submit again
- [ ] Login as approver and approve
- [ ] Navigate back to request - should show Approved readonly view
- [ ] Verify no actions available for Approved/Rejected

## Files Summary

### New Files
1. `ProcureHub/Features/PurchaseRequests/WithdrawPurchaseRequest.cs`
2. `ProcureHub.BlazorApp/Components/Pages/Requests/View.razor`

### Modified Files
1. `ProcureHub/Models/PurchaseRequest.cs`
2. `ProcureHub/Features/PurchaseRequests/Validation/PurchaseRequestErrors.cs`
3. `ProcureHub.WebApi/Features/PurchaseRequests/Endpoints.cs`
4. `ProcureHub.WebApi.Tests/Features/PurchaseRequestTests.cs`
5. `ProcureHub.BlazorApp/Components/Pages/Requests/Index.razor` (minor)
