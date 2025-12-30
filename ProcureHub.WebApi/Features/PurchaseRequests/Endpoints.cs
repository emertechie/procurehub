using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;
using ProcureHub.Common.Pagination;
using ProcureHub.Features.PurchaseRequests;
using ProcureHub.Infrastructure;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using ProcureHub.WebApi.Responses;

namespace ProcureHub.WebApi.Features.PurchaseRequests;

public static class Endpoints
{
    public static void ConfigurePurchaseRequestEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithTags("PurchaseRequests");

        group.MapPost("/purchase-requests", async (
                [FromServices] IRequestHandler<CreatePurchaseRequest.Request, Result<Guid>> handler,
                [FromBody] CreatePurchaseRequest.Request request,
                ClaimsPrincipal user,
                CancellationToken token
            ) =>
            {
                var requesterUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(requesterUserId))
                {
                    return Results.Unauthorized();
                }

                var requestWithUser = request with { RequesterUserId = requesterUserId };
                var result = await handler.HandleAsync(requestWithUser, token);
                return result.Match(
                    newId => Results.Created($"/purchase-requests/{newId}", new EntityCreatedResponse<string>(newId.ToString())),
                    error => error.ToProblemDetails());
            })
            .RequireAuthorization(RolePolicyNames.Requester)
            .WithName(nameof(CreatePurchaseRequest))
            .Produces<EntityCreatedResponse<string>>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/purchase-requests", async (
                [FromServices] IRequestHandler<QueryPurchaseRequests.Request, Result<PagedResult<QueryPurchaseRequests.Response>>> handler,
                ClaimsPrincipal user,
                CancellationToken token,
                [FromQuery] Models.PurchaseRequestStatus? status,
                [FromQuery] string? search,
                [FromQuery] int? page,
                [FromQuery] int? pageSize
            ) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var request = new QueryPurchaseRequests.Request(status, search, page, pageSize, userId);
                var result = await handler.HandleAsync(request, token);
                return result.Match(
                    pagedResult => Results.Ok(PagedResponse.From(pagedResult)),
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(QueryPurchaseRequests))
            .Produces<DataResponse<PagedResult<QueryPurchaseRequests.Response>>>()
            .ProducesValidationProblem();

        group.MapGet("/purchase-requests/{id:guid}", async (
                [FromServices] IRequestHandler<GetPurchaseRequestById.Request, Result<GetPurchaseRequestById.Response>> handler,
                ClaimsPrincipal user,
                CancellationToken token,
                Guid id
            ) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var result = await handler.HandleAsync(new GetPurchaseRequestById.Request(id, userId), token);
                return result.Match(
                    response => Results.Ok(DataResponse.From(response)),
                    error => error.ToProblemDetails()
                );
            })
            .WithName(nameof(GetPurchaseRequestById))
            .Produces<DataResponse<GetPurchaseRequestById.Response>>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/purchase-requests/{id:guid}", async (
                [FromServices] IRequestHandler<UpdatePurchaseRequest.Request, Result> handler,
                [FromBody] UpdatePurchaseRequest.Request request,
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
            .RequireAuthorization(RolePolicyNames.Requester)
            .WithName(nameof(UpdatePurchaseRequest))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/purchase-requests/{id:guid}/submit", async (
                [FromServices] IRequestHandler<SubmitPurchaseRequest.Request, Result> handler,
                CancellationToken token,
                Guid id
            ) =>
            {
                var result = await handler.HandleAsync(new SubmitPurchaseRequest.Request(id), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .RequireAuthorization(RolePolicyNames.Requester)
            .WithName(nameof(SubmitPurchaseRequest))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/purchase-requests/{id:guid}/approve", async (
                [FromServices] IRequestHandler<ApprovePurchaseRequest.Request, Result> handler,
                ClaimsPrincipal user,
                CancellationToken token,
                Guid id
            ) =>
            {
                var reviewerUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(reviewerUserId))
                {
                    return Results.Unauthorized();
                }

                var result = await handler.HandleAsync(new ApprovePurchaseRequest.Request(id, reviewerUserId), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .RequireAuthorization(RolePolicyNames.Approver)
            .WithName(nameof(ApprovePurchaseRequest))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/purchase-requests/{id:guid}/reject", async (
                [FromServices] IRequestHandler<RejectPurchaseRequest.Request, Result> handler,
                ClaimsPrincipal user,
                CancellationToken token,
                Guid id
            ) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var result = await handler.HandleAsync(new RejectPurchaseRequest.Request(id, userId), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .RequireAuthorization(RolePolicyNames.Approver)
            .WithName(nameof(RejectPurchaseRequest))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/purchase-requests/{id:guid}", async (
                [FromServices] IRequestHandler<DeletePurchaseRequest.Request, Result> handler,
                CancellationToken token,
                Guid id
            ) =>
            {
                var result = await handler.HandleAsync(new DeletePurchaseRequest.Request(id), token);
                return result.Match(
                    Results.NoContent,
                    error => error.ToProblemDetails()
                );
            })
            .RequireAuthorization(RolePolicyNames.Requester)
            .WithName(nameof(DeletePurchaseRequest))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
