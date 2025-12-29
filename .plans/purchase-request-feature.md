# Plan: Purchase Request Feature API

Add purchase request management system with models for requests and categories, supporting multi-status workflows (Draft, Pending, Approved, Rejected) and amount-based approval rules.

## Steps

### Category Feature

1. **Create Category.cs model** with: `Id` (Guid), `Name` (string, unique), `CreatedAt`, `UpdatedAt`. Include `IEntityTypeConfiguration` with unique constraint on Name.

2. **Seed Category data** in DataSeeder:
```
export const categories = [
  "IT Equipment",
  "IT Infrastructure",
  "Software",
  "Office Equipment",
  "Marketing",
  "Travel",
  "Training",
  "Professional Services",
  "Office Supplies",
  "Other",
]
```

3. **Create Category handlers** in ProcureHub/Features/Categories/: `CreateCategory.cs`, `QueryCategories.cs`, `GetCategoryById.cs`, `UpdateCategory.cs`, `DeleteCategory.cs` (prevent if has requests). Add `CategoryErrors.cs`.

4. **Map Category endpoints** in ProcureHub.WebApi/Features/Categories/Endpoints.cs. Use `/categories` route with appropriate HTTP verbs. Register in ApiEndpoints.cs.

5. **Add Category tests** in ProcureHub.WebApi.Tests/Features/CategoryTests.cs. Test CRUD operations, unique constraint on name, prevent deletion if has requests. Add `GetAllCategoryEndpoints()` method for cross-cutting tests.

### Purchase Request Feature

6. **Create PurchaseRequest.cs model** with properties: `Id` (Guid), `RequestNumber` (string, auto-generated like "PR-2024-001" using DB trigger, unique), `Title`, `Description`, `EstimatedAmount` (decimal), `BusinessJustification`, `CategoryId` (Guid FK), `DepartmentId` (Guid FK), `RequesterId` (string FK to User), `Status` (enum: Draft/Pending/Approved/Rejected), `SubmittedAt` (DateTime?), `ReviewedAt` (DateTime?), `ReviewedById` (string? FK to User), `CreatedAt`, `UpdatedAt`. Include navigation properties: `Category`, `Department`, `Requester`, `ReviewedBy`. Include `IEntityTypeConfiguration`.

7. **Create PurchaseRequest handlers** in ProcureHub/Features/PurchaseRequests/: `CreatePurchaseRequest.cs` (saves as Draft), `SubmitPurchaseRequest.cs` (transitions Draft→Pending, sets SubmittedAt, validates required fields), `QueryPurchaseRequests.cs` (with pagination, status filtering, search by title/ID), `GetPurchaseRequestById.cs`, `UpdatePurchaseRequest.cs` (only for Draft status), `ApprovePurchaseRequest.cs` (Pending→Approved, sets ReviewedAt/ReviewedById), `RejectPurchaseRequest.cs` (Pending→Rejected), `DeletePurchaseRequest.cs` (hard delete only for Draft status). Add `PurchaseRequestErrors.cs` for validation errors.

8. **Map PurchaseRequest endpoints** in ProcureHub.WebApi/Features/PurchaseRequests/Endpoints.cs. Use `/purchase-requests` route with appropriate HTTP verbs. Add command endpoints: `POST /purchase-requests/{id}/submit`, `POST /purchase-requests/{id}/approve`, `POST /purchase-requests/{id}/reject`. Register in ApiEndpoints.cs.

9. **Add PurchaseRequest tests** in ProcureHub.WebApi.Tests/Features/PurchaseRequestTests.cs. Test status workflows (Draft→Pending→Approved/Rejected), validation (no update if approved, no submit if already pending), authorization checks, and state transitions. Test hard delete only allowed for Draft status. Add `GetAllPurchaseRequestEndpoints()` method for cross-cutting tests.

10. **Seed sample purchase requests** in DataSeeder: Add sample purchase requests across different statuses, categories, and departments to demonstrate workflows.

11. **Address TODOs**: Remove TODOs added during Category feature implementation:
    - In `ProcureHub/Features/Categories/DeleteCategory.cs`: Uncomment purchase request check before deletion
    - In `ProcureHub.WebApi.Tests/Features/CategoryTests.cs`: Implement `Cannot_delete_category_with_purchase_requests` test

## Implementation Notes

- **Request Number Generation**: Use PostgreSQL trigger to auto-generate "PR-YYYY-NNN" format. Trigger should find max number for current year and increment.
- **Approval Rules**: Amount-based approval logic (≤€1k auto-approve, €1k-€10k dept manager, >€10k finance) will be tackled later. For now, just track approval status without enforcing rules.
- **Delete Policy**: Hard delete only allowed for Draft status. Prevent deletion of Pending/Approved/Rejected requests.
