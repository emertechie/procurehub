using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Users;

public static class CreateUser
{
    public record Request(string Email, string Password, string FirstName, string LastName);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Email).NotEmpty().EmailAddress();
            RuleFor(r => r.Password).NotEmpty();
            RuleFor(r => r.FirstName).NotEmpty();
            RuleFor(r => r.LastName).NotEmpty();
        }
    }

    public class Handler(
        UserManager<User> userManager,
        ILogger<Handler> logger)
        : IRequestHandler<Request, Result<string>>
    {
        public async Task<Result<string>> HandleAsync(Request request, CancellationToken token)
        {
            var now = DateTime.UtcNow;

            // Create the ASP.NET Identity user
            var user = new User
            {
                UserName = request.Email,
                // Ensure email always stored in lowercase to enable case-insensitive search
                Email = request.Email.ToLowerInvariant(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = now,
                UpdatedAt = now,
                EnabledAt = now
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to create user. Errors: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return Result.Failure<string>(UserCreationFailed(result.Errors));
            }

            var userId = await userManager.GetUserIdAsync(user);
            logger.LogInformation("Created user {UserId}", userId);
            return Result.Success(userId);
        }

        private static Error UserCreationFailed(IEnumerable<IdentityError> identityErrors)
        {
            var validationErrors = identityErrors
                .GroupBy(e => MapErrorCodeToFieldName(e.Code))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray()
                );

            return Error.Validation(
                "Staff.UserCreationFailed",
                "Failed to create staff user account",
                validationErrors
            );
        }

        private static string MapErrorCodeToFieldName(string errorCode) => errorCode switch
        {
            "PasswordRequiresNonAlphanumeric" or
            "PasswordRequiresDigit" or
            "PasswordRequiresLower" or
            "PasswordRequiresUpper" or
            "PasswordTooShort" or
            "PasswordRequiresUniqueChars" => "Password",

            "DuplicateUserName" or
            "DuplicateEmail" or
            "InvalidEmail" or
            "InvalidUserName" => "Email",

            _ => errorCode
        };
    }
}
