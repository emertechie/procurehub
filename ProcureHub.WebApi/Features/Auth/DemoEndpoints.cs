using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ProcureHub.WebApi.Features.Auth;

public static class DemoEndpoints
{
    private static readonly string[] demoUserRoles = new[] { "Admin", "Requester", "Approver" };

    public static void ConfigureDemoEndpoints(this WebApplication app)
    {
        var demoModeEnabled = app.Configuration.GetValue<bool>("ENABLE_DEMO_MODE");
        if (!demoModeEnabled)
        {
            return;
        }

        app.MapGet("/demo-users", (IConfiguration configuration) =>
            {
                var demoUsers = demoUserRoles
                    .Select(role => new DemoUser
                    {
                        Role = role,
                        Email = configuration[$"SeedUsers:{role}:Email"]!,
                        FirstName = configuration[$"SeedUsers:{role}:FirstName"]!,
                        LastName = configuration[$"SeedUsers:{role}:LastName"]!
                    })
                    .ToList();

                return Results.Ok(demoUsers);
            })
            .WithName("GetDemoUsers")
            .WithTags("Auth")
            .Produces<List<DemoUser>>();

        app.MapPost("/demo-login", async (
                [FromBody] DemoLoginRequest request,
                [FromServices] UserManager<Models.User> userManager,
                [FromServices] SignInManager<Models.User> signInManager,
                [FromServices] IConfiguration configuration) =>
            {
                // Validate that the email belongs to a known demo user
                var isDemoUser = false;
                string? password = null;

                foreach (var role in demoUserRoles)
                {
                    var configEmail = configuration[$"SeedUsers:{role}:Email"];
                    if (configEmail?.Equals(request.Email, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        isDemoUser = true;
                        password = configuration[$"SeedUsers:{role}:Password"];
                        break;
                    }
                }

                if (!isDemoUser || password == null)
                {
                    return Results.Problem(
                        title: "Invalid demo user",
                        detail: "The provided email is not a valid demo user.",
                        statusCode: StatusCodes.Status400BadRequest);
                }

                // Find the user
                var user = await userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Results.Problem(
                        title: "User not found",
                        detail: "Demo user not found in database. Database may need seeding.",
                        statusCode: StatusCodes.Status404NotFound);
                }

                // Sign in the user (using cookie authentication)
                await signInManager.SignInAsync(user, isPersistent: true);

                return Results.Ok(new { success = true });
            })
            .WithName("DemoLogin")
            .WithTags("Auth")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public sealed record DemoUser
    {
        public required string Role { get; init; }
        public required string Email { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
    }

    public sealed record DemoLoginRequest
    {
        public required string Email { get; init; }
    }
}
