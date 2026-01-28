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
            RuleFor(r => r.Page).GreaterThanOrEqualTo(1);
            RuleFor(r => r.PageSize).InclusiveBetween(1, Paging.MaxPageSize);
        }
    }

    public record Response(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        string[] Roles,
        DateTime? EnabledAt,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? DeletedAt,
        Department? Department);

    public record Department(Guid Id, string Name);

    public class Handler(ApplicationDbContext dbContext, UserManager<User> userManager)
        : IQueryHandler<Request, PagedResult<Response>>
    {
        public async Task<PagedResult<Response>> HandleAsync(Request request, CancellationToken token)
        {
            var normalizedEmail = userManager.NormalizeEmail(request.Email);

            var query = dbContext.Users
                .AsNoTracking()
                .Where(s => string.IsNullOrWhiteSpace(request.Email) ||
                            s.NormalizedEmail!.StartsWith(normalizedEmail!));

            return await query
                .OrderBy(u => u.Email)
                .ToPagedResultAsync(
                    u => new Response(
                        u.Id,
                        u.Email!,
                        u.FirstName!,
                        u.LastName!,
                        u.UserRoles!.Select(ur => ur.Role.Name!).ToArray(),
                        u.EnabledAt,
                        u.CreatedAt,
                        u.UpdatedAt,
                        u.DeletedAt,
                        u.Department != null
                            ? new Department(u.Department.Id, u.Department.Name!)
                            : null
                    ),
                    request.Page,
                    request.PageSize,
                    token);
        }
    }
}
