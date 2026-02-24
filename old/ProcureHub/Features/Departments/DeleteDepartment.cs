using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Departments;

public static class DeleteDepartment
{
    public record Command(Guid Id);

    public class Handler(ApplicationDbContext dbContext)
        : ICommandHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken token)
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == command.Id, token);

            if (department is null)
            {
                return Result.Failure(Error.NotFound("Department not found"));
            }

            var usersForDept = await dbContext.Users
                .CountAsync(u => u.DepartmentId == command.Id, token);

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
