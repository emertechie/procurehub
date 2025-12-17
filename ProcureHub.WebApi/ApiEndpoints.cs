using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Common.Pagination;
using ProcureHub.Features.Departments;
using ProcureHub.Features.Staff;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.ApiResponses;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace ProcureHub.WebApi;

public static class ApiEndpoints
{
    public static void Configure(WebApplication app)
    {
        // TODO: remove this temp endpoint to test frontend API connection
        app.MapGet("/test", async () => Results.Ok(new { DateTime = DateTime.UtcNow })) ;

        ConfigureAdditionalAuthEndpoints(app);
        ConfigureStaffEndpoints(app);
        ConfigureDepartmentEndpoints(app);
    }

    private static void ConfigureAdditionalAuthEndpoints(WebApplication app)
    {
        app.MapGet("/me", async (ClaimsPrincipal user, ILogger<WebApplication> logger) =>
            {
                if (!user.Identity?.IsAuthenticated ?? true)
                {
                    return Results.Unauthorized();
                }

                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = user.FindFirstValue(ClaimTypes.Email);

                return Results.Ok(new { id = userId, email });
            })
            .RequireAuthorization(AuthorizationPolicyNames.ApiKeyOrUserAccess)
            .WithName("GetCurrentUser")
            .WithTags("Auth");
    }

    private static void ConfigureStaffEndpoints(WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.ApiKeyOrUserAccess, RolePolicyNames.AdminOnly)
            .AddFluentValidationAutoValidation()
            .WithTags("Staff");

        group.MapPost("/staff", async (
                [FromServices] IRequestHandler<CreateStaff.Request, Result<string>> handler,
                [FromBody] CreateStaff.Request request,
                CancellationToken token
            ) =>
            {
                var result = await handler.HandleAsync(request, token);
                return result.Match(
                    onSuccess: userId => Results.Created($"/staff/{userId}", new { userId }),
                    onFailure: error => error.ToProblemDetails()
                );
            })
            .WithName("CreateStaff");

        group.MapGet("/staff", async (
                [FromServices] IRequestHandler<QueryStaff.Request, PagedResult<QueryStaff.Response>> handler,
                [AsParameters] QueryStaff.Request request,
                CancellationToken token
            ) =>
            {
                var pagedResult = await handler.HandleAsync(request, token);
                return ApiPagedResponse.From(pagedResult);
            })
            .WithName("QueryStaff");

        group.MapGet("/staff/{id}", async (
                [FromServices] IRequestHandler<GetStaffById.Request, GetStaffById.Response?> handler,
                CancellationToken token,
                string id) =>
            {
                var response = await handler.HandleAsync(new GetStaffById.Request(id), token);
                return response is not null
                    ? Results.Ok(ApiDataResponse.From(response))
                    : Results.NotFound();
            })
            .WithName("GetStaffById");
    }

    private static void ConfigureDepartmentEndpoints(WebApplication app)
    {
        app.MapPost("/departments", async (
                CreateDepartment.Request request,
                [FromServices] IRequestHandler<CreateDepartment.Request, int> handler,
                CancellationToken token
            ) =>
            {
                var newId = await handler.HandleAsync(request, token);
                return Results.Created($"/departments/{newId}", null);
            })
            .RequireAuthorization(AuthorizationPolicyNames.ApiKeyOrUserAccess)
            .WithName("CreateDepartment")
            .WithTags("Departments");

        app.MapGet("/departments", async (
                [FromServices] IRequestHandler<ListDepartments.Request, ListDepartments.Response[]> handler,
                CancellationToken token
            ) => await handler.HandleAsync(new ListDepartments.Request(), token))
            .RequireAuthorization(AuthorizationPolicyNames.ApiKeyOrUserAccess)
            .RequireAuthorization(AuthorizationPolicyNames.ApiKeyOrUserAccess)
            .WithName("GetDepartments")
            .WithTags("Departments");

        app.MapGet("/departments/{id:int}", async (
                    int id,
                    [FromServices] IRequestHandler<GetDepartment.Request, GetDepartment.Response?> handler,
                    CancellationToken token) =>
                await handler.HandleAsync(new GetDepartment.Request(id), token) is var response
                    ? Results.Ok(response)
                    : Results.NotFound())
            .RequireAuthorization(AuthorizationPolicyNames.ApiKeyOrUserAccess)
            .WithName("GetDepartmentById")
            .WithTags("Departments");
    }
}