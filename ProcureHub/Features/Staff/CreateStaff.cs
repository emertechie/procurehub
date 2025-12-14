using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Staff;

public static class CreateStaff
{
    public record Request(string Email, string Password);

    public class Handler(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger<Handler> logger)
        : IRequestHandler<Request, Result<string>>
    {
        public async Task<Result<string>> HandleAsync(Request request, CancellationToken token)
        {
            // Create the ASP.NET Identity user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                // Ensure email always stored in lowercase to enable case-insensitive search
                Email = request.Email.ToLowerInvariant()
            };
            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to create staff user with email {Email}. Errors: {Errors}",
                    request.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return Result.Failure<string>(UserCreationFailed(result.Errors));
            }
            
            var userId = await userManager.GetUserIdAsync(user);

            // Create linked Staff entity
            var now = DateTime.UtcNow;
            var staff = new Models.Staff
            {
                UserId = userId,
                CreatedAt = now,
                UpdatedAt = now,
                EnabledAt = now
            };

            dbContext.Staff.Add(staff);
            await dbContext.SaveChangesAsync(token);

            logger.LogInformation("Registered Staff entity for user {UserId}", userId);

            return Result.Success(userId);
        }
    }
    
    private static Error UserCreationFailed(IEnumerable<IdentityError> identityErrors)
    {
        var validationErrors = identityErrors
            .GroupBy(e => e.Code)
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
}
