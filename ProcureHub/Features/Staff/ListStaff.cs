using Microsoft.EntityFrameworkCore;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Staff;

public static class ListStaff
{
    public record Request();

    public record Response(
        string Id,
        string Email,
        int? DepartmentId,
        string? DepartmentName);

    // TODO: validator

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Response[]>
    {
        public Task<Response[]> HandleAsync(Request request, CancellationToken token)
        {
            return dbContext.Staff
                .AsNoTracking()
                .Include(s => s.Department)
                .Where(s => s.User.UserRoles.Any(ur => ur.Role.Name == "Staff"))
                .Select(s => new Response(
                    s.UserId,
                    s.User.Email!,
                    s.DepartmentId,
                    s.Department != null ? s.Department.Name : null))
                .ToArrayAsync(token);
        }
    }
}