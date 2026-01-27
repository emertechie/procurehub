using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.PurchaseRequests.Validation;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.PurchaseRequests;

public static class DeletePurchaseRequest
{
    public record Request(Guid Id);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
        }
    }

    public class Handler(ApplicationDbContext dbContext) : ICommandHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var purchaseRequest = await dbContext.PurchaseRequests
                .FirstOrDefaultAsync(pr => pr.Id == request.Id, token);

            if (purchaseRequest is null)
            {
                return Result.Failure(PurchaseRequestErrors.NotFound);
            }

            var result = purchaseRequest.CanDelete();
            if (result.IsFailure)
            {
                return result;
            }

            dbContext.PurchaseRequests.Remove(purchaseRequest);
            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
