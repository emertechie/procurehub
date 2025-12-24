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
                .FirstOrDefaultAsync(d => d.Id == request.Id, token);

            if (department is null)
            {
                return Result.Failure(Error.NotFound("Department not found"));
            }

            var usersForDept = await dbContext.Users
                .CountAsync(u => u.DepartmentId == request.Id, token);

            if (usersForDept > 0)
            {
                return Result.Failure(Error.Validation(
                    $"Cannot delete department. It has {usersForDept} user(s). Please reassign users before deleting."));
            }

            dbContext.Departments.Remove(department);
            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
