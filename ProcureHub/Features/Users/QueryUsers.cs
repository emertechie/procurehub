using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common.Pagination;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Users;

public static class QueryUsers
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

    public class Handler(ApplicationDbContext dbContext, UserManager<User> userManager)
        : IRequestHandler<Request, PagedResult<Response>>
    {
        public async Task<PagedResult<Response>> HandleAsync(Request request, CancellationToken token)
        {
            var normalizedEmail = userManager.NormalizeEmail(request.Email);

            var query = dbContext.Users
                .AsNoTracking()
                .Where(s => string.IsNullOrWhiteSpace(request.Email) ||
                            s.NormalizedEmail == normalizedEmail);

            return await query
                .OrderBy(s => s.Email)
                .ToPagedResultAsync(
                    s => new Response(
                        s.Id,
                        s.Email!,
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
