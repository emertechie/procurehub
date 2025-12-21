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
                UserManager<ApplicationUser> userManager) =>
            {
                if (!user.Identity?.IsAuthenticated ?? true)
                {
                    return Results.Unauthorized();
                }

                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = user.FindFirstValue(ClaimTypes.Email);

                // Get user's roles
                var appUser = await userManager.FindByIdAsync(userId);
                var roles = appUser != null ? await userManager.GetRolesAsync(appUser) : [];

                return Results.Ok(new { id = userId, email, roles });
            })
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .WithName("GetCurrentUser")
            .WithTags("Auth");
    }
}
