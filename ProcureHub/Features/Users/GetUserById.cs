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
        int? DepartmentId,
        string? DepartmentName);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Response?>
    {
        public Task<Response?> HandleAsync(Request request, CancellationToken token)
        {
            return dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == request.Id)
                .Include(u => u.Department)
                .Select(u => new Response(
                    u.Id,
                    u.Email!,
                    u.FirstName!,
                    u.LastName!,
                    u.DepartmentId,
                    u.Department != null ? u.Department.Name : null))
                .FirstOrDefaultAsync(token);
        }
    }
}
