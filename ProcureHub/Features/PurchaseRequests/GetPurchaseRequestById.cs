using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Constants;
using ProcureHub.Features.PurchaseRequests.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.PurchaseRequests;

public static class GetPurchaseRequestById
{
    public record Request(Guid Id, string UserId);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
        }
    }

    public record Response(
        Guid Id,
        string RequestNumber,
        string Title,
        string? Description,
        decimal EstimatedAmount,
        string? BusinessJustification,
        CategoryInfo Category,
        DepartmentInfo Department,
        RequesterInfo Requester,
        PurchaseRequestStatus Status,
        DateTime? SubmittedAt,
        DateTime? ReviewedAt,
        ReviewerInfo? ReviewedBy,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record CategoryInfo(Guid Id, string Name);
    public record DepartmentInfo(Guid Id, string Name);
    public record RequesterInfo(string Id, string Email, string FirstName, string LastName);
    public record ReviewerInfo(string Id, string Email, string FirstName, string LastName);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Result<Response>>
    {
        public async Task<Result<Response>> HandleAsync(Request request, CancellationToken token)
        {
            // Get user context with roles and department
            var user = await dbContext.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, token);

            if (user is null)
                return Result.Failure<Response>(PurchaseRequestErrors.Unauthorized);

            var userRoles = user.UserRoles!.Select(ur => ur.Role.Name!).ToArray();
            var isAdmin = userRoles.Contains(RoleNames.Admin);
            var isApprover = userRoles.Contains(RoleNames.Approver);

            var purchaseRequest = await dbContext.PurchaseRequests
                .AsNoTracking()
                .Include(pr => pr.Category)
                .Include(pr => pr.Department)
                .Include(pr => pr.Requester)
                .Include(pr => pr.ReviewedBy)
                .FirstOrDefaultAsync(pr => pr.Id == request.Id, token);

            if (purchaseRequest is null)
                return Result.Failure<Response>(PurchaseRequestErrors.NotFound);

            // Apply role-based authorization
            if (!isAdmin)
            {
                if (isApprover && user.DepartmentId.HasValue)
                {
                    // Approvers with department can see department requests + own requests
                    var canAccess = purchaseRequest.DepartmentId == user.DepartmentId.Value
                        || purchaseRequest.RequesterId == request.UserId;
                    if (!canAccess)
                        return Result.Failure<Response>(PurchaseRequestErrors.NotFound);
                }
                else
                {
                    // Requesters, and approvers without department, can only see their own requests
                    if (purchaseRequest.RequesterId != request.UserId)
                        return Result.Failure<Response>(PurchaseRequestErrors.NotFound);
                }
            }

            var response = new Response(
                purchaseRequest.Id,
                purchaseRequest.RequestNumber,
                purchaseRequest.Title,
                purchaseRequest.Description,
                purchaseRequest.EstimatedAmount,
                purchaseRequest.BusinessJustification,
                new CategoryInfo(purchaseRequest.Category.Id, purchaseRequest.Category.Name),
                new DepartmentInfo(purchaseRequest.Department.Id, purchaseRequest.Department.Name),
                new RequesterInfo(
                    purchaseRequest.Requester.Id,
                    purchaseRequest.Requester.Email!,
                    purchaseRequest.Requester.FirstName!,
                    purchaseRequest.Requester.LastName!),
                purchaseRequest.Status,
                purchaseRequest.SubmittedAt,
                purchaseRequest.ReviewedAt,
                purchaseRequest.ReviewedBy != null
                    ? new ReviewerInfo(
                        purchaseRequest.ReviewedBy.Id,
                        purchaseRequest.ReviewedBy.Email!,
                        purchaseRequest.ReviewedBy.FirstName!,
                        purchaseRequest.ReviewedBy.LastName!)
                    : null,
                purchaseRequest.CreatedAt,
                purchaseRequest.UpdatedAt
            );

            return Result.Success(response);
        }
    }
}
