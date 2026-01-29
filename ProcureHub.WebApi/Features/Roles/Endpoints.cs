using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Features.Roles;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Infrastructure;
using ProcureHub.WebApi.Responses;

namespace ProcureHub.WebApi.Features.Roles;

public static class Endpoints
{
    public static void ConfigureRolesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated, RolePolicyNames.Admin)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithTags("Roles");

        group.MapGet("/roles", async (
                [FromServices] IQueryHandler<QueryRoles.Request, QueryRoles.Role[]> handler,
                CancellationToken token
            ) =>
            {
                var roles = await handler.HandleAsync(new QueryRoles.Request(), token);
                return DataResponse.From(roles);
            })
            .WithName(nameof(QueryRoles))
            .Produces<DataResponse<QueryRoles.Role[]>>();

        group.MapPost("/users/{userId}/roles", async (
                [FromServices] ICommandHandler<AssignRole.Command, Result> handler,
                [FromBody] AssignRole.Command command,
                CancellationToken token,
                string userId
            ) =>
            {
                if (userId != command.UserId)
                {
                    return CustomResults.RouteIdMismatch();
                }

                var result = await handler.HandleAsync(command, token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(AssignRole))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/users/{userId}/roles/{roleId}", async (
                [FromServices] ICommandHandler<RemoveRole.Command, Result> handler,
                CancellationToken token,
                string userId,
                string roleId
            ) =>
            {
                var result = await handler.HandleAsync(new RemoveRole.Command(userId, roleId), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(RemoveRole))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
    }
}
