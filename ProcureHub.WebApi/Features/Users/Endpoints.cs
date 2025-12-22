using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Common.Pagination;
using ProcureHub.Features.Users;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.ApiResponses;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
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
                    newUserId => Results.Created($"/users/{newUserId}", new { userId = newUserId }),
                    error => error.ToProblemDetails()
                );
            })
            .WithName("CreateUsers")
            .Produces<string>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/users", async (
                [FromServices] IRequestHandler<QueryUsers.Request, PagedResult<QueryUsers.Response>> handler,
                [AsParameters] QueryUsers.Request request,
                CancellationToken token
            ) =>
            {
                var pagedResult = await handler.HandleAsync(request, token);
                return ApiPagedResponse.From(pagedResult);
            })
            .WithName("QueryUsers")
            .Produces<ApiPagedResponse<QueryUsers.Response>>();

        group.MapGet("/users/{id}", async (
                [FromServices] IRequestHandler<GetUserById.Request, GetUserById.Response?> handler,
                CancellationToken token,
                string id) =>
            {
                var response = await handler.HandleAsync(new GetUserById.Request(id), token);
                return response is not null
                    ? Results.Ok(ApiDataResponse.From(response))
                    : Results.NotFound();
            })
            .WithName("GetUserById")
            .Produces<GetUserById.Response>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
