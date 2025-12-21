using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ProcureHub.Models;

namespace ProcureHub.WebApi.Features.Auth;

public static class Endpoints
{
    public static void ConfigureAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/me", async (
                ClaimsPrincipal user,
                UserManager<ApplicationUser> userManager,
                ILogger<WebApplication> logger) =>
            {
                if (!user.Identity?.IsAuthenticated ?? true)
                {
                    return Results.Unauthorized();
                }

                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    logger.LogWarning("No name claim value found");
                    return Results.Unauthorized();
                }

                var email = user.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    logger.LogWarning("No email claim value found");
                    return Results.Unauthorized();
                }

                // Get user's roles
                var appUser = await userManager.FindByIdAsync(userId);
                if (appUser == null)
                {
                    logger.LogWarning("No user found by ID");
                    return Results.Unauthorized();
                }

                var roles = await userManager.GetRolesAsync(appUser);

                return Results.Ok(new User { Id = userId, Email = email, Roles = roles.ToArray() });
            })
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .WithName("GetCurrentUser")
            .WithTags("Auth")
            .Produces<User>();
    }
}
