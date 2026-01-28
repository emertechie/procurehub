using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Common.Pagination;
using ProcureHub.Features.Users;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using ProcureHub.WebApi.Responses;

namespace ProcureHub.WebApi.Features.Users;

public static class Endpoints
{
    public static void ConfigureUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated, RolePolicyNames.Admin)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithTags("Users");

        group.MapPost("/users", async (
                [FromServices] ICommandHandler<CreateUser.Command, Result<string>> handler,
                [FromBody] CreateUser.Command command,
                CancellationToken token
            ) =>
            {
                var result = await handler.HandleAsync(command, token);
                return result.Match(
                    newUserId => Results.Created($"/users/{newUserId}", new EntityCreatedResponse<string>(newUserId)),
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(CreateUser))
            .Produces<EntityCreatedResponse<string>>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/users", async (
                [FromServices] IQueryHandler<QueryUsers.Request, PagedResult<QueryUsers.Response>> handler,
                [AsParameters] QueryUsers.Request request,
                CancellationToken token
            ) =>
            {
                var pagedResult = await handler.HandleAsync(request, token);
                return PagedResponse.From(pagedResult);
            })
            .WithName(nameof(QueryUsers))
            .Produces<PagedResponse<QueryUsers.Response>>();

        group.MapGet("/users/{id}", async (
                [FromServices] IQueryHandler<GetUserById.Request, GetUserById.Response?> handler,
                CancellationToken token,
                string id) =>
            {
                var response = await handler.HandleAsync(new GetUserById.Request(id), token);
                return response is not null
                    ? Results.Ok(DataResponse.From(response))
                    : Results.NotFound();
            })
            .WithName(nameof(GetUserById))
            .Produces<DataResponse<GetUserById.Response>>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/users/{id}", async (
                [FromServices] ICommandHandler<UpdateUser.Command, Result> handler,
                [FromBody] UpdateUser.Command command,
                CancellationToken token,
                string id
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
            .WithName(nameof(UpdateUser))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/users/{id}/enable", async (
                [FromServices] ICommandHandler<EnableUser.Command, Result> handler,
                CancellationToken token,
                string id
            ) =>
            {
                var result = await handler.HandleAsync(new EnableUser.Command(id), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(EnableUser))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/users/{id}/disable", async (
                [FromServices] ICommandHandler<DisableUser.Command, Result> handler,
                CancellationToken token,
                string id
            ) =>
            {
                var result = await handler.HandleAsync(new DisableUser.Command(id), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(DisableUser))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/users/{id}/department", async (
                [FromServices] ICommandHandler<AssignUserToDepartment.Command, Result> handler,
                [FromBody] AssignUserToDepartment.Command command,
                CancellationToken token,
                string id
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
            .WithName(nameof(AssignUserToDepartment))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
    }
}
