using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Common.Pagination;
using ProcureHub.Features.Departments;
using ProcureHub.Features.Staff;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.ApiResponses;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;

namespace ProcureHub.WebApi;

public static class ApiEndpoints
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
        
    public static void Configure(WebApplication app)
    {
        ConfigureStaffEndpoints(app);
        ConfigureDepartmentEndpoints(app);
    }

    private static void ConfigureStaffEndpoints(WebApplication app)
    {
        app.MapPost("/staff", async (
                [FromServices] IRequestHandler<CreateStaff.Request, Result<string>> handler,
                CancellationToken token,
                CreateStaff.Request request
            ) =>
            {
                var result = await handler.HandleAsync(request, token);
                return result.Match(
                    onSuccess: userId => Results.Created($"/staff/{userId}", new { userId }),
                    onFailure: error => error.ToProblemDetails()
                );
            })
            .RequireAuthorization(AuthorizationPolicyNames.ApiKeyOrUserAccess, RolePolicyNames.AdminOnly)
            .WithName("CreateStaff")
            .WithTags("Staff");

        app.MapGet("/staff", async (
                [FromServices] IRequestHandler<ListStaff.Request, PagedResult<ListStaff.Response>> handler,
                CancellationToken token,
                string? email,
                int page = DefaultPage,
                int pageSize = DefaultPageSize
            ) =>
            {
                var pagedResult = await handler.HandleAsync(new ListStaff.Request(email, page, pageSize), token);
                return ApiPagedResponse.From(pagedResult);
            })
            .RequireAuthorization(AuthorizationPolicyNames.ApiKeyOrUserAccess, RolePolicyNames.AdminOnly)
            .WithName("GetStaff")
            .WithTags("Staff");

        app.MapGet("/staff/{id}", async (
                [FromServices] IRequestHandler<GetStaff.Request, GetStaff.Response?> handler,
                CancellationToken token,
                string id) =>
            {
                var response = await handler.HandleAsync(new GetStaff.Request(id), token);
                return response is not null
                    ? Results.Ok(ApiDataResponse.From(response))
                    : Results.NotFound();
            })
            .RequireAuthorization(AuthorizationPolicyNames.ApiKeyOrUserAccess, RolePolicyNames.AdminOnly)
            .WithName("GetStaffById")
            .WithTags("Staff");
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