using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Users;

public static class AssignUserToDepartment
{
    public record Command(string Id, Guid? DepartmentId);

    internal class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
        }
    }

    internal class Handler(
        ApplicationDbContext dbContext,
        ILogger<Handler> logger,
        Metrics metrics,
        Instrumentation instrumentation)
        : ICommandHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken token)
        {
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == command.Id, token);

            if (user is null)
            {
                return Result.Failure(Error.NotFound(
                    "User.NotFound",
                    $"User with ID '{command.Id}' not found"));
            }

            // If a department is specified, verify it exists
            if (command.DepartmentId.HasValue)
            {
                var departmentExists = await dbContext.Departments
                    .AnyAsync(d => d.Id == command.DepartmentId.Value, token);

                if (!departmentExists)
                {
                    return Result.Failure(Error.NotFound(
                        "Department.NotFound",
                        $"Department with ID '{command.DepartmentId}' not found"));
                }
            }

            var oldDepartmentId = user.DepartmentId;
            metrics.DepartmentChanged(oldDepartmentId, command.DepartmentId);

            user.DepartmentId = command.DepartmentId;
            user.UpdatedAt = DateTime.UtcNow;

            using (var activity = instrumentation.ActivitySource.StartActivity("AssignUserToDepartment"))
            {
                activity?.SetTag("user.id", user.Id);
                activity?.SetTag("old_department_id", oldDepartmentId);
                activity?.SetTag("new_department_id", command.DepartmentId);

                // Simulate some work
                await Task.Delay(TimeSpan.FromMilliseconds(500), token);

                await dbContext.SaveChangesAsync(token);
            }
            
            logger.LogInformation("Assigned user {UserId} to department {DepartmentId}",
                user.Id, command.DepartmentId?.ToString() ?? "null");
            return Result.Success();
        }
    }
}
