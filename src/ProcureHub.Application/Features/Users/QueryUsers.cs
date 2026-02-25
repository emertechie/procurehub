using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Common.Pagination;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.Users;

public static class QueryUsers
{
    public record Request(string? Email, int? Page, int? PageSize) : IRequest<PagedResult<Response>>;

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
        IReadOnlyCollection<string> Roles,
        DateTime? EnabledAt,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? DeletedAt,
        DepartmentInfo? Department);

    public record DepartmentInfo(Guid Id, string Name);

    public class Handler(IApplicationDbContext dbContext, UserManager<User> userManager)
        : IRequestHandler<Request, PagedResult<Response>>
    {
        public async Task<PagedResult<Response>> HandleAsync(Request request, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(request);
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
                            ? new DepartmentInfo(u.Department.Id, u.Department.Name!)
                            : null
                    ),
                    request.Page,
                    request.PageSize,
                    token);
        }
    }
}
