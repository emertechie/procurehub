using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Domain.Common;

namespace ProcureHub.Application.Features.Departments;

public static class DeleteDepartment
{
    public record Command(Guid Id) : IRequest<Result>;

    public class Handler(IApplicationDbContext dbContext)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken);

            if (department is null)
            {
                return Result.Failure(Error.NotFound("Department not found"));
            }

            var usersForDept = await dbContext.Users
                .CountAsync(u => u.DepartmentId == command.Id, cancellationToken);

            if (usersForDept > 0)
            {
                return Result.Failure(Error.Validation(
                    $"Cannot delete department. It has {usersForDept} user(s). Please reassign users before deleting."));
            }

            dbContext.Departments.Remove(department);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
