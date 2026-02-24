using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Abstractions.Identity;
using ProcureHub.Application.Common;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;
using ProcureHub.Application.Common.Pagination;
using ProcureHub.Application.Constants;
using ProcureHub.Application.Features.PurchaseRequests.Extensions;

namespace ProcureHub.Application.Features.PurchaseRequests;

public static class QueryPurchaseRequests
{
    public record Request(
        PurchaseRequestStatus? Status,
        string? Search,
        Guid? DepartmentId,
        int? Page,
        int? PageSize,
        string UserId
    );

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Page).GreaterThanOrEqualTo(1);
            RuleFor(r => r.PageSize).InclusiveBetween(1, Paging.MaxPageSize);
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

    public class Handler(IApplicationDbContext dbContext, ICurrentUserProvider currentUserProvider)
        : IQueryHandler<Request, Result<PagedResult<Response>>>
    {
        public async Task<Result<PagedResult<Response>>> HandleAsync(Request request, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(request);
            var currentUser = await currentUserProvider.GetCurrentUserAsync();

            if (!currentUser.UserId.HasValue)
            {
                return Result.Failure<PagedResult<Response>>(Error.Unauthorized());
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
                .WhereUserAllowedToViewPurchaseRequest(currentUserId, currentUserDeptId, isAdmin, isApprover);

            if (request.Status.HasValue)
            {
                query = query.Where(pr => pr.Status == request.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(pr =>
                    pr.Title.Contains(request.Search) ||
                    pr.RequestNumber.Contains(request.Search)
                );
            }

            if (request.DepartmentId.HasValue)
            {
                query = query.Where(pr => pr.DepartmentId == request.DepartmentId.Value);
            }

            var pagedResult = await query
                .OrderByDescending(pr => pr.CreatedAt)
                .ToPagedResultAsync(
                    pr => new Response(
                        pr.Id,
                        pr.RequestNumber,
                        pr.Title,
                        pr.Description,
                        pr.EstimatedAmount,
                        pr.BusinessJustification,
                        new CategoryInfo(pr.Category.Id, pr.Category.Name),
                        new DepartmentInfo(pr.Department.Id, pr.Department.Name),
                        new RequesterInfo(pr.Requester.Id, pr.Requester.Email!, pr.Requester.FirstName!, pr.Requester.LastName!),
                        pr.Status,
                        pr.SubmittedAt,
                        pr.ReviewedAt,
                        pr.ReviewedBy != null
                            ? new ReviewerInfo(pr.ReviewedBy.Id, pr.ReviewedBy.Email!, pr.ReviewedBy.FirstName!, pr.ReviewedBy.LastName!)
                            : null,
                        pr.CreatedAt,
                        pr.UpdatedAt
                    ),
                    request.Page,
                    request.PageSize,
                    token);

            return Result.Success(pagedResult);
        }
    }
}
