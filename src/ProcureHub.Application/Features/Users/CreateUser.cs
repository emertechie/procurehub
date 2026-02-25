using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProcureHub.Application.Common;
using ProcureHub.Application.Features.Users.Validation;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.Users;

public static class CreateUser
{
    public record Command(string Email, string Password, string FirstName, string LastName) : IRequest<Result<string>>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Email).NotEmpty().EmailAddress();
            RuleFor(r => r.Password).NotEmpty();
            RuleFor(r => r.FirstName).NotEmpty().MaximumLength(User.FirstNameMaxLength);
            RuleFor(r => r.LastName).NotEmpty().MaximumLength(User.LastNameMaxLength);
        }
    }

    public class Handler(
        UserManager<User> userManager,
        ILogger<Handler> logger)
        : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            var now = DateTime.UtcNow;

            var user = new User
            {
                UserName = command.Email,
                Email = User.NormalizeEmailForDisplay(command.Email),
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
