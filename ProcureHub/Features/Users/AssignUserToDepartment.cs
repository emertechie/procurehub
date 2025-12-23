using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Users;

public static class AssignUserToDepartment
{
    public record Request(string Id, int? DepartmentId);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
        }
    }

    public class Handler(
        ApplicationDbContext dbContext,
        ILogger<Handler> logger)
        : IRequestHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id, token);

            if (user is null)
            {
                return Result.Failure(Error.NotFound(
                    "User.NotFound",
                    $"User with ID '{request.Id}' not found"));
            }

            // If a department is specified, verify it exists
            if (request.DepartmentId.HasValue)
            {
                var departmentExists = await dbContext.Departments
                    .AnyAsync(d => d.Id == request.DepartmentId.Value, token);

                if (!departmentExists)
                {
                    return Result.Failure(Error.NotFound(
                        "Department.NotFound",
                        $"Department with ID '{request.DepartmentId}' not found"));
                }
            }

            user.DepartmentId = request.DepartmentId;
            user.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(token);

            logger.LogInformation("Assigned user {UserId} to department {DepartmentId}",
                user.Id, request.DepartmentId?.ToString() ?? "null");
            return Result.Success();
        }
    }
}
