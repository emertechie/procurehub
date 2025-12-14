using Microsoft.EntityFrameworkCore;
using ProcureHub.Common.Pagination;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Staff;

public static class ListStaff
{
    public record Request(string? Email, int Page, int PageSize);

    public record Response(
        string Id,
        string Email,
        int? DepartmentId,
        string? DepartmentName);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, PagedResult<Response>>
    {
        public Task<PagedResult<Response>> HandleAsync(Request request, CancellationToken token)
        {
            var query = dbContext.Staff
                .AsNoTracking()
                .Where(s => string.IsNullOrWhiteSpace(request.Email) || s.User.Email == request.Email.ToLowerInvariant());

            return query
                .OrderBy(s => s.User.Email)
                .ToPagedResultAsync(request.Page, request.PageSize,
                    staff => new Response(
                        staff.UserId,
                        staff.User.Email!,
                        staff.DepartmentId,
                        staff.Department != null ? staff.Department.Name : null),
                    token);
        }
    }
}