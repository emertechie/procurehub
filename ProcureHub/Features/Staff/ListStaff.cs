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
            var query = dbContext.Staff.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                // Note: emails always stored in lowercase
                var lowerCasedEmail = request.Email.ToLowerInvariant();
                query = query.Where(s => s.User.Email == lowerCasedEmail);    
            }

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