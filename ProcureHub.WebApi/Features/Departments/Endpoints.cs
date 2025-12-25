using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Features.Departments;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using ProcureHub.WebApi.Responses;
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
                [FromServices] IRequestHandler<CreateDepartment.Request, Guid> handler,
                [FromBody] CreateDepartment.Request request,
                CancellationToken token
            ) =>
            {
                var newId = await handler.HandleAsync(request, token);
                return Results.Created($"/departments/{newId}", new EntityCreatedResponse<string>(newId.ToString()));
            })
            .RequireAuthorization(RolePolicyNames.AdminOnly)
            .WithName(nameof(CreateDepartment))
            .Produces<EntityCreatedResponse<string>>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/departments", async (
                [FromServices] IRequestHandler<QueryDepartments.Request, QueryDepartments.Response[]> handler,
                CancellationToken token
            ) =>
            {
                var departments = await handler.HandleAsync(new QueryDepartments.Request(), token);
                return Results.Ok(DataResponse.From(departments));
            })
            .WithName(nameof(QueryDepartments))
            .Produces<DataResponse<QueryDepartments.Response[]>>();

        group.MapGet("/departments/{id:guid}", async (
                [FromServices] IRequestHandler<GetDepartmentById.Request, GetDepartmentById.Response?> handler,
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
                [FromServices] IRequestHandler<UpdateDepartment.Request, Result> handler,
                [FromBody] UpdateDepartment.Request request,
                CancellationToken token,
                Guid id
            ) =>
            {
                if (id != request.Id)
                {
                    return CustomResults.RouteIdMismatch();
                }

                var result = await handler.HandleAsync(request, token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .RequireAuthorization(RolePolicyNames.AdminOnly)
            .WithName(nameof(UpdateDepartment))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/departments/{id:guid}", async (
                [FromServices] IRequestHandler<DeleteDepartment.Request, Result> handler,
                CancellationToken token,
                Guid id
            ) =>
            {
                var result = await handler.HandleAsync(new DeleteDepartment.Request(id), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .RequireAuthorization(RolePolicyNames.AdminOnly)
            .WithName(nameof(DeleteDepartment))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
