using ProcureHub.Common;

namespace ProcureHub.Features.PurchaseRequests;

public static class PurchaseRequestErrors
{
    public static readonly Error NotFound = Error.NotFound("Purchase request not found");
    public static readonly Error InvalidStatus = Error.Validation("PurchaseRequest.InvalidStatusTransition", "Invalid status transition", new Dictionary<string, string[]>
    {
        ["Status"] = ["Invalid status for this operation"]
    });
    public static readonly Error CannotSubmitNonDraft = Error.Validation("PurchaseRequest.InvalidStatusTransition", "Invalid status transition", new Dictionary<string, string[]>
    {
        ["Status"] = ["Cannot submit a purchase request that is not in Draft status."]
    });
    public static readonly Error CannotUpdateNonDraft = Error.Validation("PurchaseRequest.CannotUpdateNonDraft", "Cannot update submitted request", new Dictionary<string, string[]>
    {
        ["Status"] = ["Only purchase requests in Draft status can be updated."]
    });
    public static readonly Error CannotApproveNonPending = Error.Validation("PurchaseRequest.InvalidStatusTransition", "Invalid status transition", new Dictionary<string, string[]>
    {
        ["Status"] = ["Can only approve purchase requests in Pending status."]
    });
    public static readonly Error CannotRejectNonPending = Error.Validation("PurchaseRequest.InvalidStatusTransition", "Invalid status transition", new Dictionary<string, string[]>
    {
        ["Status"] = ["Can only reject purchase requests in Pending status."]
    });
    public static readonly Error CannotDeleteNonDraft = Error.Validation("PurchaseRequest.CannotDeleteNonDraft", "Cannot delete non-draft request", new Dictionary<string, string[]>
    {
        ["Status"] = ["Only purchase requests in Draft status can be deleted."]
    });
    public static readonly Error CategoryNotFound = Error.Validation("PurchaseRequest.CategoryNotFound", "Category not found", new Dictionary<string, string[]>
    {
        ["CategoryId"] = ["The specified category does not exist."]
    });
    public static readonly Error DepartmentNotFound = Error.Validation("PurchaseRequest.DepartmentNotFound", "Department not found", new Dictionary<string, string[]>
    {
        ["DepartmentId"] = ["The specified department does not exist."]
    });
}
