using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Common.Pagination;
using ProcureHub.Constants;
using ProcureHub.Features.PurchaseRequests.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.PurchaseRequests;

public static class QueryPurchaseRequests
{
    public record Request(
        PurchaseRequestStatus? Status,
        string? Search,
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

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Result<PagedResult<Response>>>
    {
        public async Task<Result<PagedResult<Response>>> HandleAsync(Request request, CancellationToken token)
        {
            // Get user context with roles and department
            var user = await dbContext.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, token);

            if (user is null)
                return Result.Failure<PagedResult<Response>>(PurchaseRequestErrors.Unauthorized);

            var userRoles = user.UserRoles!.Select(ur => ur.Role.Name!).ToArray();
            var isAdmin = userRoles.Contains(RoleNames.Admin);
            var isApprover = userRoles.Contains(RoleNames.Approver);

            var query = dbContext.PurchaseRequests
                .AsNoTracking()
                .Include(pr => pr.Category)
                .Include(pr => pr.Department)
                .Include(pr => pr.Requester)
                .Include(pr => pr.ReviewedBy)
                .AsQueryable();

            // Apply role-based filtering
            if (!isAdmin)
            {
                if (isApprover && user.DepartmentId.HasValue)
                {
                    // Approvers with department see department requests + own requests
                    query = query.Where(pr => pr.DepartmentId == user.DepartmentId.Value
                        || pr.RequesterId == request.UserId);
                }
                else
                {
                    // Requesters and approvers without department see only their own requests
                    query = query.Where(pr => pr.RequesterId == request.UserId);
                }
            }

            if (request.Status.HasValue)
            {
                query = query.Where(pr => pr.Status == request.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(pr =>
                    pr.Title.ToLower().Contains(search) ||
                    pr.RequestNumber.ToLower().Contains(search));
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
