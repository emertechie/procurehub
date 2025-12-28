using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Users;

public static class GetUserById
{
    public record Request(string Id);

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
        string[] Roles,
        DateTime? EnabledAt,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? DeletedAt,
        Department? Department);

    public record Department(Guid Id, string Name);

    public class Handler(ApplicationDbContext dbContext)
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
                        ? new Department(u.Department.Id, u.Department.Name!)
                        : null
                ))
                .FirstOrDefaultAsync(token);
        }
    }
}
