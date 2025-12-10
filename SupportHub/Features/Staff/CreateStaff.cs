using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SupportHub.Data;
using SupportHub.Infrastructure;

namespace SupportHub.Features.Staff;

public static class CreateStaff
{
    public record Request(string Email, string Password);

    public record Response(bool Succeeded, IEnumerable<IdentityError> Errors, string? UserId);

    // TODO: validator

    public class Handler(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        // IUserStore<ApplicationUser> userStore,
        ILogger<Handler> logger)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> HandleAsync(Request request, CancellationToken token)
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
                // await userStore.SetUserNameAsync(user, request.Email, token);
                // await EmailStore.SetEmailAsync(user, request.Email, token);
                var result = await userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    logger.LogWarning("Failed to create staff user with email {Email}. Errors: {Errors}",
                        request.Email,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return new Response(false, result.Errors, null);
                }

                var userId = await userManager.GetUserIdAsync(user);

                // Create the Staff record
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

                logger.LogInformation("Created staff user with email {Email} and userId {UserId}",
                    request.Email,
                    userId);

                return new Response(true, Enumerable.Empty<IdentityError>(), userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating staff user with email {Email}", request.Email);
                await transaction.RollbackAsync(token);
                throw;
            }
        }

        /*private IUserEmailStore<ApplicationUser> EmailStore
        {
            get
            {
                if (!userManager.SupportsUserEmail)
                {
                    throw new NotSupportedException("The user store must support user email.");
                }
                return (IUserEmailStore<ApplicationUser>)userStore;
            }
        }*/
    }
}
