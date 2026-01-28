using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Features.Categories;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using ProcureHub.WebApi.Responses;

namespace ProcureHub.WebApi.Features.Categories;

public static class Endpoints
{
    public static void ConfigureCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithTags("Categories");

        group.MapPost("/categories", async (
                [FromServices] ICommandHandler<CreateCategory.Command, Result<Guid>> handler,
                [FromBody] CreateCategory.Command command,
                CancellationToken token
            ) =>
            {
                var result = await handler.HandleAsync(command, token);
                return result.Match(
                    newId => Results.Created($"/categories/{newId}", new EntityCreatedResponse<string>(newId.ToString())),
                    error => error.ToProblemDetails());
            })
            .RequireAuthorization(RolePolicyNames.Admin)
            .WithName(nameof(CreateCategory))
            .Produces<EntityCreatedResponse<string>>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/categories", async (
                [FromServices] IQueryHandler<QueryCategories.Request, QueryCategories.Response[]> handler,
                CancellationToken token
            ) =>
            {
                var categories = await handler.HandleAsync(new QueryCategories.Request(), token);
                return Results.Ok(DataResponse.From(categories));
            })
            .WithName(nameof(QueryCategories))
            .Produces<DataResponse<QueryCategories.Response[]>>();

        group.MapGet("/categories/{id:guid}", async (
                [FromServices] IQueryHandler<GetCategoryById.Request, Result<GetCategoryById.Response>> handler,
                CancellationToken token,
                Guid id
            ) =>
            {
                var result = await handler.HandleAsync(new GetCategoryById.Request(id), token);
                return result.Match(
                    response => Results.Ok(DataResponse.From(response)),
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(GetCategoryById))
            .Produces<DataResponse<GetCategoryById.Response>>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/categories/{id:guid}", async (
                [FromServices] ICommandHandler<UpdateCategory.Command, Result> handler,
                [FromBody] UpdateCategory.Command command,
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
            .WithName(nameof(UpdateCategory))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/categories/{id:guid}", async (
                [FromServices] ICommandHandler<DeleteCategory.Command, Result> handler,
                CancellationToken token,
                Guid id
            ) =>
            {
                var result = await handler.HandleAsync(new DeleteCategory.Command(id), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .RequireAuthorization(RolePolicyNames.Admin)
            .WithName(nameof(DeleteCategory))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
