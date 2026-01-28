using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Features.Users.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Users;

public static class CreateUser
{
    public record Command(string Email, string Password, string FirstName, string LastName);

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Email).NotEmpty().EmailAddress();
            RuleFor(r => r.Password).NotEmpty();
            RuleFor(r => r.FirstName).NotEmpty().MaximumLength(UserConfiguration.FirstNameMaxLength);
            RuleFor(r => r.LastName).NotEmpty().MaximumLength(UserConfiguration.LastNameMaxLength);
        }
    }

    public class Handler(
        UserManager<User> userManager,
        ILogger<Handler> logger)
        : ICommandHandler<Command, Result<string>>
    {
        public async Task<Result<string>> HandleAsync(Command command, CancellationToken token)
        {
            var now = DateTime.UtcNow;

            // Create the ASP.NET Identity user
            var user = new User
            {
                UserName = command.Email,
                // Ensure email always stored in lowercase to enable case-insensitive search
                Email = command.Email.ToLowerInvariant(),
                FirstName = command.FirstName,
                LastName = command.LastName,
                CreatedAt = now,
                UpdatedAt = now,
                EnabledAt = now
            };

            var result = await userManager.CreateAsync(user, command.Password);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to create user. Errors: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return Result.Failure<string>(
                    IdentityErrorMapper.ToValidationError(
                        result.Errors,
                        "User.CreationFailed",
                        "Failed to create user account"));
            }

            var userId = await userManager.GetUserIdAsync(user);
            logger.LogInformation("Created user {UserId}", userId);
            return Result.Success(userId);
        }
    }
}
