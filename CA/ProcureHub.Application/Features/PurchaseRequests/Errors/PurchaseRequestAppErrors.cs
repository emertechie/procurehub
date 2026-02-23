using ProcureHub.Domain.Common;

namespace ProcureHub.Application.Features.PurchaseRequests.Errors;

public static class PurchaseRequestAppErrors
{
    public static readonly Error NotFound = Error.NotFound("Purchase request not found");
    public static readonly Error CategoryNotFound = Error.Validation("PurchaseRequest.CategoryNotFound", "Category not found", new Dictionary<string, string[]>
    {
        ["CategoryId"] = ["The specified category does not exist."]
    });
    public static readonly Error DepartmentNotFound = Error.Validation("PurchaseRequest.DepartmentNotFound", "Department not found", new Dictionary<string, string[]>
    {
        ["DepartmentId"] = ["The specified department does not exist."]
    });
    public static readonly Error Unauthorized = Error.Unauthorized("PurchaseRequest.Unauthorized", "You are not authorized to access this purchase request");
}
