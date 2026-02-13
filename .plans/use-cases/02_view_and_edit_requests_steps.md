# View/Edit Purchase Requests - Implementation Steps

## Phase 1: Backend Implementation

### Step 1: Add Withdraw method to PurchaseRequest model
- [ ] Open `ProcureHub/Models/PurchaseRequest.cs`
- [ ] Add `Withdraw()` method after `CanDelete()` method (around line 101)
- [ ] Method should:
  - Check if Status is Pending, return failure if not
  - Set Status to Draft
  - Set SubmittedAt to null
  - Set UpdatedAt to DateTime.UtcNow
  - Return Result.Success()

### Step 2: Add CannotWithdrawNonPending error
- [ ] Open `ProcureHub/Features/PurchaseRequests/Validation/PurchaseRequestErrors.cs`
- [ ] Add `CannotWithdrawNonPending` error after `CannotDeleteNonDraft`
- [ ] Use similar pattern: Error.Validation with appropriate title, detail, and Status field error

### Step 3: Create WithdrawPurchaseRequest command handler
- [ ] Create new file `ProcureHub/Features/PurchaseRequests/WithdrawPurchaseRequest.cs`
- [ ] Add Command record with Id property
- [ ] Add CommandValidator with Id.NotEmpty() rule
- [ ] Add Handler that:
  - Loads purchase request by Id
  - Returns NotFound if doesn't exist
  - Calls pr.Withdraw()
  - Saves changes if successful
  - Returns result

### Step 4: Add Withdraw endpoint
- [ ] Open `ProcureHub.WebApi/Features/PurchaseRequests/Endpoints.cs`
- [ ] Add MapPost endpoint for `/purchase-requests/{id:guid}/withdraw` after Delete endpoint
- [ ] Follow pattern of other command endpoints
- [ ] Require Requester role authorization
- [ ] Produces 204 NoContent and 404 NotFound

### Step 5: Add API tests for Withdraw endpoint
- [ ] Open `ProcureHub.WebApi.Tests/Features/PurchaseRequestTests.cs`
- [ ] Add withdraw endpoint to `GetAllPurchaseRequestEndpoints` theory data
- [ ] Add `Test_WithdrawPurchaseRequest_validation` method
- [ ] Add `Can_withdraw_pending_purchase_request` test
- [ ] Add `Cannot_withdraw_draft_purchase_request` test  
- [ ] Add `Cannot_withdraw_approved_purchase_request` test
- [ ] Add `Cannot_withdraw_rejected_purchase_request` test

## Phase 2: UI Implementation

### Step 6: Create View/Edit Request page
- [ ] Create new file `ProcureHub.BlazorApp/Components/Pages/Requests/View.razor`
- [ ] Add `@page "/requests/{Id:guid}"` route
- [ ] Add `[Authorize]` attribute
- [ ] Inject required handlers:
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
- [ ] Add PageTitle and PageHeader
- [ ] Create two-column layout (8/4 ratio like New.razor)
- [ ] Left column: Request Details card
- [ ] Right column: Actions card

### Step 8: Implement loading state
- [ ] Add `_loading` field
- [ ] Show RadzenProgressBarCircular while loading
- [ ] Load request data in OnInitializedAsync
- [ ] Load categories and departments for dropdowns

### Step 9: Implement Draft view (editable)
- [ ] When Status == Draft:
  - Show EditForm with FluentValidator
  - Show editable fields: Title, Description, Category, Department, Amount, Justification
  - Actions: "Save Changes", "Submit for Approval", "Delete"

### Step 10: Implement Pending view (readonly with withdraw)
- [ ] When Status == Pending:
  - Show readonly display of all fields (RadzenText, not form inputs)
  - Show status badge prominently
  - Show SubmittedAt date
  - Actions: "Withdraw Request" button

### Step 11: Implement Approved/Rejected view (readonly)
- [ ] When Status is Approved or Rejected:
  - Show readonly display of all fields
  - Show status badge prominently
  - Show SubmittedAt, ReviewedAt, ReviewedBy info
  - No action buttons

### Step 12: Implement action handlers
- [ ] SaveChanges method: calls Update handler
- [ ] SubmitRequest method: calls Submit handler, navigates on success
- [ ] WithdrawRequest method: calls Withdraw handler, refreshes data on success
- [ ] DeleteRequest method: calls Delete handler, navigates on success
- [ ] All methods show notifications on success/failure

### Step 13: Update Requests list navigation
- [ ] Open `ProcureHub.BlazorApp/Components/Pages/Requests/Index.razor`
- [ ] Verify view button navigates to `/requests/{pr.Id}`
- [ ] Consider making RequestNumber clickable as well

## Phase 3: Testing and Validation

### Step 14: Build and verify
- [ ] Run `dotnet build` to ensure no compilation errors
- [ ] Run API tests to verify withdraw functionality
- [ ] Verify all tests pass

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
