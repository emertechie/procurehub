using ProcureHub.WebApi.Features.Auth;
using ProcureHub.WebApi.Features.Departments;
using ProcureHub.WebApi.Features.Roles;
using ProcureHub.WebApi.Features.Users;

namespace ProcureHub.WebApi;

public static class ApiEndpoints
{
    public static void Configure(WebApplication app)
    {
        // TODO: remove this temp endpoint to test frontend API connection
        app.MapGet("/test", async () => Results.Ok(new { DateTime = DateTime.UtcNow }));

        app.ConfigureAuthEndpoints();
        app.ConfigureUsersEndpoints();
        app.ConfigureDepartmentEndpoints();
        app.ConfigureRolesEndpoints();
    }
}
