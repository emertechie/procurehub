using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Features.Roles;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using ProcureHub.WebApi.Responses;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace ProcureHub.WebApi.Features.Roles;

public static class Endpoints
{
    public static void ConfigureRolesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated, RolePolicyNames.AdminOnly)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddFluentValidationAutoValidation()
            .WithTags("Roles");

        group.MapGet("/roles", async (
                [FromServices] IRequestHandler<QueryRoles.Request, QueryRoles.Role[]> handler,
                CancellationToken token
            ) =>
            {
                var roles = await handler.HandleAsync(new QueryRoles.Request(), token);
                return DataResponse.From(roles);
            })
            .WithName(nameof(QueryRoles))
            .Produces<DataResponse<QueryRoles.Role[]>>();

        group.MapPost("/users/{userId}/roles", async (
                [FromServices] IRequestHandler<AssignRole.Request, Result> handler,
                [FromBody] AssignRole.Request request,
                CancellationToken token,
                string userId
            ) =>
            {
                if (userId != request.UserId)
                {
                    return CustomResults.RouteIdMismatch();
                }

                var result = await handler.HandleAsync(request, token);
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
                [FromServices] IRequestHandler<RemoveRole.Request, Result> handler,
                CancellationToken token,
                string userId,
                string roleId
            ) =>
            {
                var result = await handler.HandleAsync(new RemoveRole.Request(userId, roleId), token);
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
