using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SupportHub.Common;
using SupportHub.Constants;
using SupportHub.Infrastructure;
using SupportHub.Models;

namespace SupportHub.Features.Staff;

public static class CreateStaff
{
    public record Request(string Email, string Password);

    public record Response(bool Succeeded, IEnumerable<IdentityError> Errors, string? UserId);

    // TODO: validator

    public class Handler(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger<Handler> logger)
        : IRequestHandler<Request, Result<string>>
    {
        public async Task<Result<string>> HandleAsync(Request request, CancellationToken token)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(token);

            try
            {
                // Create the ASP.NET Identity user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email
                };
                var addUserResult = await userManager.CreateAsync(user, request.Password);

                if (!addUserResult.Succeeded)
                {
                    logger.LogWarning("Failed to create staff user with email {Email}. Errors: {Errors}",
                        request.Email,
                        string.Join(", ", addUserResult.Errors.Select(e => e.Description)));
                    return Result.Failure<string>(StaffErrors.UserCreationFailed(addUserResult.Errors));
                }

                var userId = await userManager.GetUserIdAsync(user);

                var addToRoleResult = await userManager.AddToRoleAsync(user, RoleNames.Staff);
                if (!addToRoleResult.Succeeded)
                {
                    logger.LogWarning("Failed to add role to staff user {UserId}. Errors: {Errors}",
                        userId,
                        string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                    return Result.Failure<string>(StaffErrors.UserCreationFailed(addToRoleResult.Errors));
                }

                // Create the Staff record - linked 1-to-1 with the ASP.Net user record
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

                await transaction.CommitAsync(token);

                logger.LogInformation("Created staff user with userId {UserId}", userId);

                return Result.Success(userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating staff user with email {Email}", request.Email);
                await transaction.RollbackAsync(token);
                throw;
            }
        }
    }
}
