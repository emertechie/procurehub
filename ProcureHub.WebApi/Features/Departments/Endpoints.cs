using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Features.Departments;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using ProcureHub.WebApi.Responses;

namespace ProcureHub.WebApi.Features.Departments;

public static class Endpoints
{
    public static void ConfigureDepartmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithTags("Departments");

        group.MapPost("/departments", async (
                [FromServices] ICommandHandler<CreateDepartment.Command, Result<Guid>> handler,
                [FromBody] CreateDepartment.Command command,
                CancellationToken token
            ) =>
            {
                var result = await handler.HandleAsync(command, token);
                return result.Match(
                    newId => Results.Created($"/departments/{newId}", new EntityCreatedResponse<string>(newId.ToString())),
                    error => error.ToProblemDetails());
            })
            .RequireAuthorization(RolePolicyNames.Admin)
            .WithName(nameof(CreateDepartment))
            .Produces<EntityCreatedResponse<string>>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/departments", async (
                [FromServices] IQueryHandler<QueryDepartments.Request, QueryDepartments.Response[]> handler,
                CancellationToken token
            ) =>
            {
                var departments = await handler.HandleAsync(new QueryDepartments.Request(), token);
                return Results.Ok(DataResponse.From(departments));
            })
            .WithName(nameof(QueryDepartments))
            .Produces<DataResponse<QueryDepartments.Response[]>>();

        group.MapGet("/departments/{id:guid}", async (
                [FromServices] IQueryHandler<GetDepartmentById.Request, GetDepartmentById.Response?> handler,
                CancellationToken token,
                Guid id
            ) =>
            {
                var response = await handler.HandleAsync(new GetDepartmentById.Request(id), token);
                return response is not null
                    ? Results.Ok(DataResponse.From(response))
                    : Results.NotFound();
            })
            .WithName(nameof(GetDepartmentById))
            .Produces<DataResponse<GetDepartmentById.Response>>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/departments/{id:guid}", async (
                [FromServices] ICommandHandler<UpdateDepartment.Command, Result> handler,
                [FromBody] UpdateDepartment.Command command,
                CancellationToken token,
                Guid id
            ) =>
            {
                if (id != command.Id)
                {
                    return CustomResults.RouteIdMismatch();
                }

                var result = await handler.HandleAsync(command, token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .RequireAuthorization(RolePolicyNames.Admin)
            .WithName(nameof(UpdateDepartment))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/departments/{id:guid}", async (
                [FromServices] ICommandHandler<DeleteDepartment.Command, Result> handler,
                CancellationToken token,
                Guid id
            ) =>
            {
                var result = await handler.HandleAsync(new DeleteDepartment.Command(id), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .RequireAuthorization(RolePolicyNames.Admin)
            .WithName(nameof(DeleteDepartment))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
