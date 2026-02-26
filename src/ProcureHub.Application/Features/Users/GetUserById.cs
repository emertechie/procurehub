using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Common.Authorization;
using ProcureHub.Application.Constants;

namespace ProcureHub.Application.Features.Users;

public static class GetUserById
{
    [AuthorizeRequest(RoleNames.Admin)]
    public record Request(string Id) : IRequest<Response?>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
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

    public class Handler(IApplicationDbContext dbContext)
        : IRequestHandler<Request, Response?>
    {
        public Task<Response?> HandleAsync(Request request, CancellationToken token)
        {
            return dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == request.Id)
                .Select(u => new Response(
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
                ))
                .FirstOrDefaultAsync(token);
        }
    }
}
