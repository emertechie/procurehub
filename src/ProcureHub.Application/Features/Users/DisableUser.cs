using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Common.Authorization;
using ProcureHub.Application.Constants;
using ProcureHub.Domain.Common;

namespace ProcureHub.Application.Features.Users;

public static class DisableUser
{
    [AuthorizeRequest(RoleNames.Admin)]
    public record Command(string Id) : IRequest<Result>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
        }
    }

    public class Handler(
        IApplicationDbContext dbContext,
        ILogger<Handler> logger)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(command);
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == command.Id, token);

            if (user is null)
            {
                return Result.Failure(Error.NotFound(
                    "User.NotFound",
                    $"User with ID '{command.Id}' not found"));
            }

            if (!user.EnabledAt.HasValue)
            {
                // Already disabled, no-op
                return Result.Success();
            }

            user.EnabledAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(token);

            logger.LogInformation("Disabled user {UserId}", user.Id);
            return Result.Success();
        }
    }
}
