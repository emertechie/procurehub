using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.Users;

public static class UpdateUser
{
    public record Command(string Id, string Email, string FirstName, string LastName) : IRequest<Result>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Email).NotEmpty().EmailAddress();
            RuleFor(r => r.FirstName).NotEmpty().MaximumLength(User.FirstNameMaxLength);
            RuleFor(r => r.LastName).NotEmpty().MaximumLength(User.LastNameMaxLength);
        }
    }

    public class Handler(
        IApplicationDbContext dbContext,
        UserManager<User> userManager,
        ILogger<Handler> logger)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

            if (user is null)
            {
                return Result.Failure(Error.NotFound(
                    "User.NotFound",
                    $"User with ID '{command.Id}' not found"));
            }

            // Update basic profile fields
            user.Email = User.NormalizeEmailForDisplay(command.Email);
            user.NormalizedEmail = userManager.NormalizeEmail(command.Email);
            user.UserName = command.Email;
            user.NormalizedUserName = userManager.NormalizeName(command.Email);
            user.FirstName = command.FirstName;
            user.LastName = command.LastName;
            user.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Updated user {UserId}", user.Id);
            return Result.Success();
        }
    }
}
