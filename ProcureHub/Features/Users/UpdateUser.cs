using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Users;

public static class UpdateUser
{
    public record Request(string Id, string Email, string FirstName, string LastName);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Email).NotEmpty().EmailAddress();
            RuleFor(r => r.FirstName).NotEmpty().MaximumLength(UserConfiguration.FirstNameMaxLength);
            RuleFor(r => r.LastName).NotEmpty().MaximumLength(UserConfiguration.LastNameMaxLength);
        }
    }

    public class Handler(
        ApplicationDbContext dbContext,
        UserManager<User> userManager,
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

            // Update basic profile fields
            user.Email = request.Email.ToLowerInvariant();
            user.NormalizedEmail = userManager.NormalizeEmail(request.Email);
            user.UserName = request.Email;
            user.NormalizedUserName = userManager.NormalizeName(request.Email);
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(token);

            logger.LogInformation("Updated user {UserId}", user.Id);
            return Result.Success();
        }
    }
}
