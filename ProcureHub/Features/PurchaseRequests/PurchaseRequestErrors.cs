using ProcureHub.Common;

namespace ProcureHub.Features.PurchaseRequests;

public static class PurchaseRequestErrors
{
    public static readonly Error NotFound = Error.NotFound("PurchaseRequest.NotFound", "Purchase request not found");
    public static readonly Error InvalidStatus = Error.Failure("PurchaseRequest.InvalidStatus", "Invalid status for this operation");
    public static readonly Error AlreadySubmitted = Error.Conflict("PurchaseRequest.AlreadySubmitted", "Purchase request already submitted");
    public static readonly Error NotDraft = Error.Failure("PurchaseRequest.NotDraft", "Can only update draft purchase requests");
    public static readonly Error NotPending = Error.Failure("PurchaseRequest.NotPending", "Can only approve/reject pending purchase requests");
    public static readonly Error MissingRequiredFields = Error.Validation("Title and estimated amount are required");
    public static readonly Error CannotDelete = Error.Failure("PurchaseRequest.CannotDelete", "Can only delete draft purchase requests");
    public static readonly Error CategoryNotFound = Error.NotFound("PurchaseRequest.CategoryNotFound", "Category not found");
    public static readonly Error DepartmentNotFound = Error.NotFound("PurchaseRequest.DepartmentNotFound", "Department not found");
}
