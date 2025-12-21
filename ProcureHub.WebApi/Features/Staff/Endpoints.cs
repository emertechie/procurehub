using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Common.Pagination;
using ProcureHub.Features.Staff;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.ApiResponses;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace ProcureHub.WebApi.Features.Staff;

public static class Endpoints
{
    public static void ConfigureStaffEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated, RolePolicyNames.AdminOnly)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddFluentValidationAutoValidation()
            .WithTags("Staff");

        group.MapPost("/staff", async (
                [FromServices] IRequestHandler<CreateStaff.Request, Result<string>> handler,
                [FromBody] CreateStaff.Request request,
                CancellationToken token
            ) =>
            {
                var result = await handler.HandleAsync(request, token);
                return result.Match(
                    newUserId => Results.Created($"/staff/{newUserId}", new { userId = newUserId }),
                    error => error.ToProblemDetails()
                );
            })
            .WithName("CreateStaff")
            .Produces<string>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/staff", async (
                [FromServices] IRequestHandler<QueryStaff.Request, PagedResult<QueryStaff.Response>> handler,
                [AsParameters] QueryStaff.Request request,
                CancellationToken token
            ) =>
            {
                var pagedResult = await handler.HandleAsync(request, token);
                return ApiPagedResponse.From(pagedResult);
            })
            .WithName("QueryStaff")
            .Produces<ApiPagedResponse<QueryStaff.Response>>();

        group.MapGet("/staff/{id}", async (
                [FromServices] IRequestHandler<GetStaffById.Request, GetStaffById.Response?> handler,
                CancellationToken token,
                string id) =>
            {
                var response = await handler.HandleAsync(new GetStaffById.Request(id), token);
                return response is not null
                    ? Results.Ok(ApiDataResponse.From(response))
                    : Results.NotFound();
            })
            .WithName("GetStaffById")
            .Produces<GetStaffById.Response>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
