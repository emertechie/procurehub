using ProcureHub.Models;

namespace ProcureHub.Features.PurchaseRequests;

public static class PurchaseRequestQueryExtensions
{
    public static IQueryable<PurchaseRequest> WhereUserAllowedToViewPurchaseRequest(
        this IQueryable<PurchaseRequest> query,
        string currentUserId,
        Guid? currentUserDeptId,
        bool isAdmin,
        bool isApprover)
    {
        if (!isAdmin)
        {
            if (isApprover && currentUserDeptId.HasValue)
            {
                // Approvers with department see department requests + own requests
                query = query.Where(pr => pr.DepartmentId == currentUserDeptId.Value
                    || pr.RequesterId == currentUserId);
            }
            else
            {
                // Requesters and approvers without department see only their own requests
                query = query.Where(pr => pr.RequesterId == currentUserId);
            }
        }

        return query;
    }
}
