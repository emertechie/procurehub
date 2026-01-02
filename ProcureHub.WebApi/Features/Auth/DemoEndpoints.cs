using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ProcureHub.WebApi.Features.Auth;

public static class DemoEndpoints
{

    public static void ConfigureDemoEndpoints(this WebApplication app)
    {
        var demoModeEnabled = app.Configuration.GetValue<bool>("ENABLE_DEMO_MODE");
        if (!demoModeEnabled)
        {
            return;
        }

        app.MapGet("/demo-users", (IConfiguration configuration) =>
            {
                var demoUserEmails = configuration.GetSection("DemoUsers").Get<string[]>() ?? Array.Empty<string>();
                var seedUsersSection = configuration.GetSection("SeedUsers");

                var demoUsers = demoUserEmails
                    .Select(email =>
                    {
                        var userSection = seedUsersSection.GetSection(email);
                        if (!userSection.Exists())
                        {
                            return null;
                        }
                        return new DemoUser
                        {
                            Email = email,
                            FirstName = userSection["FirstName"] ?? "",
                            LastName = userSection["LastName"] ?? "",
                            Roles = userSection.GetSection("Roles").GetChildren().Select(c => c.Value).OfType<string>().ToArray()
                        };
                    })
                    .OfType<DemoUser>()
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
                var demoUserEmails = configuration.GetSection("DemoUsers").Get<string[]>() ?? Array.Empty<string>();

                // Validate that the email belongs to a demo user
                if (!demoUserEmails.Contains(request.Email, StringComparer.OrdinalIgnoreCase))
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

    public sealed record SeedUserConfig
    {
        public required string Email { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public string[]? Roles { get; init; }
    }

    public sealed record DemoUser
    {
        public required string Email { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string[] Roles { get; init; }
    }

    public sealed record DemoLoginRequest
    {
        public required string Email { get; init; }
    }
}
