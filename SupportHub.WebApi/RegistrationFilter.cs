
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using SupportHub.Constants;
using SupportHub.Models;

namespace SupportHub.WebApi;

// TODO: extract handler to check if email allowed
// TODO: use domain handler to create Staff record

public class RegistrationFilter(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ILogger<RegistrationFilter> logger) : IEndpointFilter
{
    // TODO: Move this to a database table or configuration
    private static readonly HashSet<string> AllowedEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "staff1@example.com",
        "staff2@example.com",
        "test@test.com"
    };

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Get the registration request from the endpoint arguments
        var registration = context.GetArgument<RegisterRequest>(0);

        // Check if email is in allowed list
        if (!AllowedEmails.Contains(registration.Email))
        {
            logger.LogWarning("Registration attempt with unauthorized email: {Email}", registration.Email);
            
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "Email", new[] { "This email is not authorized to register." } }
            });
        }

        logger.LogInformation("Allowing registration for email: {Email}", registration.Email);

        // Allow the registration to proceed
        var result = await next(context);

        // Microsoft.AspNetCore.Http.HttpResults.Results`2[Microsoft.AspNetCore.Http.HttpResults.Ok,Microsoft.AspNetCore.Http.HttpResults.ValidationProblem]
        // If registration succeeded, create the Staff entity
        if (result is Microsoft.AspNetCore.Http.HttpResults.Results<Microsoft.AspNetCore.Http.HttpResults.Ok, Microsoft.AspNetCore.Http.HttpResults.ValidationProblem>)
        {
            await CreateStaffEntityAsync(registration.Email);
        }

        return result;
    }

    private async Task CreateStaffEntityAsync(string email)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                logger.LogWarning("User not found after registration: {Email}", email);
                return;
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
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Created Staff entity for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Staff entity for email {Email}", email);
            // Note: User account was already created. Consider cleanup or manual intervention.
        }
    }
}