using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common.Pagination;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Staff;

public static class QueryStaff
{
    public record Request(string? Email, int? Page, int? PageSize);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Email).EmailAddress();
            RuleFor(r => r.Page).GreaterThanOrEqualTo(1);
            RuleFor(r => r.PageSize).InclusiveBetween(1, Paging.MaxPageSize);
        }
    }

    public record Response(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        int? DepartmentId,
        string? DepartmentName);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, PagedResult<Response>>
    {
        public async Task<PagedResult<Response>> HandleAsync(Request request, CancellationToken token)
        {
            var query = dbContext.Staff
                .AsNoTracking()
                .Where(s => string.IsNullOrWhiteSpace(request.Email) ||
                            s.User.Email == request.Email.ToLowerInvariant());

            return await query
                .OrderBy(s => s.User.Email)
                .ToPagedResultAsync(
                    s => new Response(
                        s.UserId,
                        s.User.Email!,
                        s.FirstName!,
                        s.LastName!,
                        s.DepartmentId,
                        s.Department != null ? s.Department.Name : null),
                    request.Page,
                    request.PageSize,
                    token);
        }
    }
}