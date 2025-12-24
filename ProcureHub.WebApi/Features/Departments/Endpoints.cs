using Microsoft.AspNetCore.Mvc;
using ProcureHub.Features.Departments;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace ProcureHub.WebApi.Features.Departments;

public static class Endpoints
{
    public static void ConfigureDepartmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddFluentValidationAutoValidation()
            .WithTags("Departments");

        group.MapPost("/departments", async (
                [FromServices] IRequestHandler<CreateDepartment.Request, int> handler,
                [FromBody] CreateDepartment.Request request,
                CancellationToken token
            ) =>
            {
                var newId = await handler.HandleAsync(request, token);
                return Results.Created($"/departments/{newId}", null);
            })
            .RequireAuthorization(RolePolicyNames.AdminOnly)
            .WithName("CreateDepartment");

        group.MapGet("/departments", async (
                [FromServices] IRequestHandler<ListDepartments.Request, ListDepartments.Response[]> handler,
                CancellationToken token
            ) => await handler.HandleAsync(new ListDepartments.Request(), token))
            .WithName("GetDepartments");

        group.MapGet("/departments/{id:int}", async (
                    [FromServices] IRequestHandler<GetDepartment.Request, GetDepartment.Response?> handler,
                    CancellationToken token,
                    int id) =>
                await handler.HandleAsync(new GetDepartment.Request(id), token) is var response
                    ? Results.Ok(response)
                    : Results.NotFound())
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .WithName("GetDepartmentById");

        group.MapPut("/departments/{id:int}", async (
                [FromBody] UpdateDepartment.Request request,
                [FromServices] IRequestHandler<UpdateDepartment.Request, Common.Result> handler,
                CancellationToken token,
                int id) =>
            {
                if (id != request.Id)
                    return Results.BadRequest();

                var result = await handler.HandleAsync(request, token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails());
            })
            .RequireAuthorization(RolePolicyNames.AdminOnly)
            .WithName("UpdateDepartment");

        group.MapDelete("/departments/{id:int}", async (
                [FromServices] IRequestHandler<DeleteDepartment.Request, Common.Result> handler,
                CancellationToken token,
                int id) =>
            {
                var result = await handler.HandleAsync(new DeleteDepartment.Request(id), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails());
            })
            .RequireAuthorization(RolePolicyNames.AdminOnly)
            .WithName("DeleteDepartment");
    }
}
