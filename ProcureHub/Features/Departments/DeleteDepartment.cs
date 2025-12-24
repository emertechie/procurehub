using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Departments;

public static class DeleteDepartment
{
    public record Request(int Id);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var department = await dbContext.Departments
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == request.Id, token);

            if (department is null)
                return Result.Failure(Error.NotFound("Department not found"));

            var activeUserCount = department.Users.Count(u => u.EnabledAt.HasValue);
            if (activeUserCount > 0)
            {
                return Result.Failure(Error.Validation(
                    $"Cannot delete department. It has {activeUserCount} active user(s). Please reassign users before deleting."));
            }

            dbContext.Departments.Remove(department);
            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
