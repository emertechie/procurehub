using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Constants;
using ProcureHub.Features.PurchaseRequests.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Infrastructure.Authentication;
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

    public class Handler(ApplicationDbContext dbContext, ICurrentUserProvider currentUserProvider)
        : IQueryHandler<Request, Result<Response>>
    {
        public async Task<Result<Response>> HandleAsync(Request request, CancellationToken token)
        {
            var currentUser = await currentUserProvider.GetCurrentUserAsync();

            if (!currentUser.UserId.HasValue)
            {
                return Result.Failure<Response>(Error.Unauthorized());
            }

            var currentUserId = currentUser.UserId.Value.ToString();
            var isAdmin = currentUser.IsInRole(RoleNames.Admin);
            var isApprover = currentUser.IsInRole(RoleNames.Approver);

            Guid? currentUserDeptId = await dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == currentUserId)
                .Select(u => u.DepartmentId)
                .FirstAsync(token);

            var query = dbContext.PurchaseRequests
                .AsNoTracking()
                .Where(pr => pr.Id == request.Id)
                .WhereUserAllowedToViewPurchaseRequest(currentUserId, currentUserDeptId, isAdmin, isApprover);

            var response = await query
                .Select(pr => new Response(
                    pr.Id,
                    pr.RequestNumber,
                    pr.Title,
                    pr.Description,
                    pr.EstimatedAmount,
                    pr.BusinessJustification,
                    new CategoryInfo(pr.Category.Id, pr.Category.Name),
                    new DepartmentInfo(pr.Department.Id, pr.Department.Name),
                    new RequesterInfo(
                        pr.Requester.Id,
                        pr.Requester.Email!,
                        pr.Requester.FirstName!,
                        pr.Requester.LastName!),
                    pr.Status,
                    pr.SubmittedAt,
                    pr.ReviewedAt,
                    pr.ReviewedBy != null
                        ? new ReviewerInfo(
                            pr.ReviewedBy.Id,
                            pr.ReviewedBy.Email!,
                            pr.ReviewedBy.FirstName!,
                            pr.ReviewedBy.LastName!)
                        : null,
                    pr.CreatedAt,
                    pr.UpdatedAt
                ))
                .FirstOrDefaultAsync(token);

            return response is null
                ? Result.Failure<Response>(PurchaseRequestErrors.NotFound)
                : Result.Success(response);
        }
    }
}
