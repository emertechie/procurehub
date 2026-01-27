using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Users;

public static class EnableUser
{
    public record Request(string Id);

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
        : ICommandHandler<Request, Result>
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

            if (user.EnabledAt.HasValue)
            {
                // Already enabled, no-op
                return Result.Success();
            }

            var now = DateTime.UtcNow;
            user.EnabledAt = now;
            user.UpdatedAt = now;

            await dbContext.SaveChangesAsync(token);

            logger.LogInformation("Enabled user {UserId}", user.Id);
            return Result.Success();
        }
    }
}
