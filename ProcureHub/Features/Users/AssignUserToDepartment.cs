using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Users;

public static class AssignUserToDepartment
{
    public record Command(string Id, Guid? DepartmentId);

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
        }
    }

    public class Handler(
        ApplicationDbContext dbContext,
        ILogger<Handler> logger)
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

            user.DepartmentId = command.DepartmentId;
            user.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(token);

            logger.LogInformation("Assigned user {UserId} to department {DepartmentId}",
                user.Id, command.DepartmentId?.ToString() ?? "null");
            return Result.Success();
        }
    }
}
