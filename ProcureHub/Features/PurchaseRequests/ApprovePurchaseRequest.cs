using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.PurchaseRequests.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.PurchaseRequests;

public static class ApprovePurchaseRequest
{
    public record Request(Guid Id, string ReviewerUserId);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.ReviewerUserId).NotEmpty();
        }
    }

    public class Handler(ApplicationDbContext dbContext)
        : ICommandHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var purchaseRequest = await dbContext.PurchaseRequests
                .FirstOrDefaultAsync(pr => pr.Id == request.Id, token);

            if (purchaseRequest is null)
            {
                return Result.Failure(PurchaseRequestErrors.NotFound);
            }

            var result = purchaseRequest.Approve(request.ReviewerUserId);
            if (result.IsFailure)
            {
                return result;
            }

            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
