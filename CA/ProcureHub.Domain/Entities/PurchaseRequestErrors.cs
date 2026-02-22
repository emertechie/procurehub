using ProcureHub.Domain.Common;

namespace ProcureHub.Domain.Entities;

public static class PurchaseRequestErrors
{
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
    public static readonly Error CannotApproveOwnRequest = Error.Validation("PurchaseRequest.CannotApproveOwnRequest", "Cannot approve your own request");
    public static readonly Error CannotRejectNonPending = Error.Validation("PurchaseRequest.InvalidStatusTransition", "Invalid status transition", new Dictionary<string, string[]>
    {
        ["Status"] = ["Can only reject purchase requests in Pending status."]
    });
    public static readonly Error CannotDeleteNonDraft = Error.Validation("PurchaseRequest.CannotDeleteNonDraft", "Cannot delete non-draft request", new Dictionary<string, string[]>
    {
        ["Status"] = ["Only purchase requests in Draft status can be deleted."]
    });
    public static readonly Error CannotWithdrawNonPending = Error.Validation("PurchaseRequest.InvalidStatusTransition", "Invalid status transition", new Dictionary<string, string[]>
    {
        ["Status"] = ["Can only withdraw purchase requests in Pending status."]
    });
}
