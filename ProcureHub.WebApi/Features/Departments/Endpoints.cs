using Microsoft.AspNetCore.Mvc;
using ProcureHub.Features.Departments;
using ProcureHub.Infrastructure;

namespace ProcureHub.WebApi.Features.Departments;

public static class Endpoints
{
    public static void ConfigureDepartmentEndpoints(this WebApplication app)
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
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .WithName("CreateDepartment")
            .WithTags("Departments");

        app.MapGet("/departments", async (
                [FromServices] IRequestHandler<ListDepartments.Request, ListDepartments.Response[]> handler,
                CancellationToken token
            ) => await handler.HandleAsync(new ListDepartments.Request(), token))
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .WithName("GetDepartments")
            .WithTags("Departments");

        app.MapGet("/departments/{id:int}", async (
                    int id,
                    [FromServices] IRequestHandler<GetDepartment.Request, GetDepartment.Response?> handler,
                    CancellationToken token) =>
                await handler.HandleAsync(new GetDepartment.Request(id), token) is var response
                    ? Results.Ok(response)
                    : Results.NotFound())
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .WithName("GetDepartmentById")
            .WithTags("Departments");
    }
}
