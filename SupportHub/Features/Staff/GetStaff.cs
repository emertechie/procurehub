using Microsoft.EntityFrameworkCore;
using SupportHub.Infrastructure;

namespace SupportHub.Features.Staff;

public static class GetStaff
{
    public record Request(string Id);

    public record Response(
        string Id,
        string Email,
        int? DepartmentId,
        string? DepartmentName);

    // TODO: validator

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Response?>
    {
        public Task<Response?> HandleAsync(Request request, CancellationToken token)
        {
            return dbContext.Staff
                .AsNoTracking()
                .Where(s => s.UserId == request.Id)
                .Where(s => s.User.UserRoles.Any(ur => ur.Role.Name == "Staff"))
                .Include(s => s.User)
                .Include(s => s.Department)
                .Select(s => new Response(
                    s.UserId,
                    s.User.Email!,
                    s.DepartmentId,
                    s.Department != null ? s.Department.Name : null))
                .FirstOrDefaultAsync(token);
        }
    }
}