using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Common.Pagination;
using ProcureHub.Features.Users;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using ProcureHub.WebApi.Responses;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace ProcureHub.WebApi.Features.Users;

public static class Endpoints
{
    public static void ConfigureUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated, RolePolicyNames.AdminOnly)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddFluentValidationAutoValidation()
            .WithTags("Users");

        group.MapPost("/users", async (
                [FromServices] IRequestHandler<CreateUser.Request, Result<string>> handler,
                [FromBody] CreateUser.Request request,
                CancellationToken token
            ) =>
            {
                var result = await handler.HandleAsync(request, token);
                return result.Match(
                    newUserId => Results.Created($"/users/{newUserId}", new EntityCreatedResponse<string>(newUserId)),
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(CreateUser))
            .Produces<EntityCreatedResponse<string>>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/users", async (
                [FromServices] IRequestHandler<QueryUsers.Request, PagedResult<QueryUsers.Response>> handler,
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
                [FromServices] IRequestHandler<GetUserById.Request, GetUserById.Response?> handler,
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
                [FromServices] IRequestHandler<UpdateUser.Request, Result> handler,
                [FromBody] UpdateUser.Request request,
                CancellationToken token,
                string id
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
            .WithName(nameof(UpdateUser))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/users/{id}/enable", async (
                [FromServices] IRequestHandler<EnableUser.Request, Result> handler,
                CancellationToken token,
                string id
            ) =>
            {
                var result = await handler.HandleAsync(new EnableUser.Request(id), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(EnableUser))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/users/{id}/disable", async (
                [FromServices] IRequestHandler<DisableUser.Request, Result> handler,
                CancellationToken token,
                string id
            ) =>
            {
                var result = await handler.HandleAsync(new DisableUser.Request(id), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(DisableUser))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/users/{id}/department", async (
                [FromServices] IRequestHandler<AssignUserToDepartment.Request, Result> handler,
                [FromBody] AssignUserToDepartment.Request request,
                CancellationToken token,
                string id
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
            .WithName(nameof(AssignUserToDepartment))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
    }
}
