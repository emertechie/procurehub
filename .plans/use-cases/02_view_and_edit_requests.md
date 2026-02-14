# View/Edit Purchase Requests Use Case

## Overview

When a user clicks on a request ID in the requests grid, they can:
- View already-submitted requests (readonly view, with ability to withdraw if not already approved/rejected)
- Edit draft requests

## Current State

The following already exists:
- `GetPurchaseRequestById` - query handler for viewing a single request
- `UpdatePurchaseRequest` - command handler for editing (only works for Draft status)
- `SubmitPurchaseRequest` - command handler for submitting drafts
- `DeletePurchaseRequest` - command handler for deleting drafts
- Requests list page at `/requests` with navigation to `/requests/{id}`

## Missing Functionality

**Withdraw Request** - No endpoint/handler exists to withdraw a submitted (Pending) request.

When a request is withdrawn:
- Status changes from `Pending` back to `Draft`
- `SubmittedAt` is cleared
- Request can be edited again and re-submitted

## Implementation Plan

### 1. Domain Layer (ProcureHub)

#### 1.1 Add Withdraw method to PurchaseRequest model

Add `Withdraw()` method to `Models/PurchaseRequest.cs` that:
- Validates request is in `Pending` status
- Resets status to `Draft`
- Clears `SubmittedAt`
- Updates `UpdatedAt`

#### 1.2 Add validation error for withdraw

Add `CannotWithdrawNonPending` error to `Features/PurchaseRequests/Validation/PurchaseRequestErrors.cs`

#### 1.3 Create WithdrawPurchaseRequest command handler

Create `Features/PurchaseRequests/WithdrawPurchaseRequest.cs`:
- Command with `Id` parameter
- Handler that loads request, calls `Withdraw()`, saves changes
- Returns `Result` (success or failure)

### 2. API Layer (ProcureHub.WebApi)

#### 2.1 Add Withdraw endpoint

Add `POST /purchase-requests/{id}/withdraw` endpoint to `Features/PurchaseRequests/Endpoints.cs`:
- Requires `Requester` role
- Calls WithdrawPurchaseRequest handler
- Returns 204 on success

### 3. UI Layer (ProcureHub.BlazorApp)

#### 3.1 Create View/Edit Request page

Create `Components/Pages/Requests/View.razor` at route `/requests/{Id:guid}`:

**For Draft requests:**
- Display form with editable fields (same as New.razor)
- Show "Save Changes" button
- Show "Submit for Approval" button
- Show "Delete" button

**For Pending requests:**
- Display readonly view of all fields
- Show "Withdraw Request" button (returns to Draft)
- Show status badge prominently

**For Approved/Rejected requests:**
- Display readonly view of all fields
- Show status badge prominently
- Show approval/rejection info (date, reviewer)
- No action buttons (view only)

**Page Structure:**
- Two-column layout (same as New.razor)
- Left column: Request details form or readonly display
- Right column: Actions card (context-sensitive based on status)

### 4. Testing

#### 4.1 Add Withdraw endpoint to test theory data

Update `GetAllPurchaseRequestEndpoints` in `PurchaseRequestTests.cs` to include withdraw endpoint

#### 4.2 Add validation test for withdraw

Add `Test_WithdrawPurchaseRequest_validation` method

#### 4.3 Add withdraw functionality tests

Add tests for:
- Can withdraw pending request (returns to Draft, SubmittedAt cleared)
- Cannot withdraw draft request (validation error)
- Cannot withdraw approved request (validation error)
- Cannot withdraw rejected request (validation error)

#### 4.4 Add authorization tests

- Only request owner or admin can withdraw their pending request

## UI Design

### Draft State
```
┌─────────────────────────────────────────────────────────────┐
│ Purchase Request #PR-00001                              │
│ Edit draft request                                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────────────────────┐  ┌───────────────┐   │
│  │ Request Details                  │  │ Actions       │   │
│  │                                  │  │               │   │
│  │ Title: [_________________]       │  │ [Save Changes]│   │
│  │                                  │  │               │   │
│  │ Description:                     │  │ [Submit for   │   │
│  │ [_________________________]      │  │  Approval]    │   │
│  │ [_________________________]      │  │               │   │
│  │                                  │  │ [Delete]      │   │
│  │ Category: [Dropdown_______]      │  │               │   │
│  │ Department: [Dropdown_____]      │  │               │   │
│  │                                  │  │               │   │
│  │ Estimated Amount: [€____]        │  │               │   │
│  │                                  │  │               │   │
│  │ Business Justification:          │  │               │   │
│  │ [_________________________]      │  │               │   │
│  │                                  │  │               │   │
│  └──────────────────────────────────┘  └───────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Pending State (Readonly with Withdraw)
```
┌─────────────────────────────────────────────────────────────┐
│ Purchase Request #PR-00001                    [PENDING]     │
│ Submitted: 2025-02-13                                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────────────────────┐  ┌───────────────┐   │
│  │ Request Details                  │  │ Actions       │   │
│  │                                  │  │               │   │
│  │ Title: MacBook Pro               │  │ [Withdraw]    │   │
│  │                                  │  │               │   │
│  │ Description:                     │  │ Return to     │   │
│  │ Need laptop for dev              │  │ draft status  │   │
│  │                                  │  │ to edit       │   │
│  │ Category: IT Equipment           │  │               │   │
│  │ Department: Engineering          │  │               │   │
│  │                                  │  │               │   │
│  │ Estimated Amount: €1,500.00      │  │               │   │
│  │                                  │  │               │   │
│  │ Business Justification:          │  │               │   │
│  │ New hire equipment               │  │               │   │
│  │                                  │  │               │   │
│  └──────────────────────────────────┘  └───────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Approved/Rejected State (Readonly only)
```
┌─────────────────────────────────────────────────────────────┐
│ Purchase Request #PR-00001                    [APPROVED]    │
│ Submitted: 2025-02-13                                       │
│ Approved: 2025-02-13 by John Smith                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────────────────────┐                       │
│  │ Request Details                  │                       │
│  │                                  │                       │
│  │ Title: MacBook Pro               │                       │
│  │                                  │                       │
│  │ ... (all fields readonly)        │                       │
│  │                                  │                       │
│  └──────────────────────────────────┘                       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## State Machine

```
                    ┌─────────┐
         ┌─────────►│  Draft  │◄──────────┐
         │          └────┬────┘           │
         │               │ Update          │ Withdraw
         │               │ (edit fields)   │
         │               ▼                 │
         │          ┌─────────┐            │
   Delete│    Submit│ Pending │            │
         │          └────┬────┘            │
         │               │                 │
         │        ┌──────┴──────┐          │
         │        ▼             ▼          │
         │   ┌─────────┐   ┌─────────┐     │
         └───┤Approved │   │Rejected │─────┘
             └─────────┘   └─────────┘
```

## Tests to Write

1. **API Tests** (PurchaseRequestTests.cs)
   - Cross-cutting tests: Add withdraw endpoint to `GetAllPurchaseRequestEndpoints`
   - Validation test: `Test_WithdrawPurchaseRequest_validation`
   - `Can_withdraw_pending_purchase_request`
   - `Cannot_withdraw_draft_purchase_request`
   - `Cannot_withdraw_approved_purchase_request`
   - `Cannot_withdraw_rejected_purchase_request`
   - `Cannot_withdraw_nonexistent_purchase_request`

2. **Unit Tests** (optional)
   - `PurchaseRequest.Withdraw_ResetsStatusToDraft`
   - `PurchaseRequest.Withdraw_ClearsSubmittedAt`
   - `PurchaseRequest.Withdraw_ThrowsForNonPendingStatus`

## Files to Modify

1. `ProcureHub/Models/PurchaseRequest.cs` - Add Withdraw method
2. `ProcureHub/Features/PurchaseRequests/Validation/PurchaseRequestErrors.cs` - Add CannotWithdrawNonPending
3. `ProcureHub/Features/PurchaseRequests/WithdrawPurchaseRequest.cs` - New file
4. `ProcureHub.WebApi/Features/PurchaseRequests/Endpoints.cs` - Add withdraw endpoint
5. `ProcureHub.WebApi.Tests/Features/PurchaseRequestTests.cs` - Add tests
6. `ProcureHub.BlazorApp/Components/Pages/Requests/View.razor` - New file
7. `ProcureHub.BlazorApp/Components/Pages/Requests/Index.razor` - Update view button to navigate

## Success Criteria

- [ ] User can view any of their requests at `/requests/{id}`
- [ ] Draft requests show editable form with Save, Submit, Delete actions
- [ ] Pending requests show readonly view with Withdraw action
- [ ] Approved/Rejected requests show readonly view with no actions
- [ ] Withdrawing a pending request returns it to Draft status
- [ ] All endpoints require authentication
- [ ] Only request owners/admins can view/edit their requests
- [ ] All tests pass
