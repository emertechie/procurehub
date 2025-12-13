using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SupportHub.Common;
using SupportHub.Constants;
using SupportHub.Infrastructure;
using SupportHub.Models;

namespace SupportHub.Features.Staff.Registration;

public static class RegisterStaff
{
    public record Request(string Email);

    public class Handler(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger<Handler> logger)
        : IRequestHandler<Request, Result<string>>
    {
        public async Task<Result<string>> HandleAsync(Request request, CancellationToken token)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new InvalidOperationException($"User record not found during registration: {request.Email}");
            }

            var userId = await userManager.GetUserIdAsync(user);

            // Assign Staff role
            await userManager.AddToRoleAsync(user, RoleNames.Staff);

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
}
