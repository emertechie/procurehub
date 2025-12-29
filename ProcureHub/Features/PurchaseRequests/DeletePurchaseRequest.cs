using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.PurchaseRequests;

public static class DeletePurchaseRequest
{
    public record Request(Guid Id);

    public class Handler(ApplicationDbContext dbContext) : IRequestHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var purchaseRequest = await dbContext.PurchaseRequests
                .FirstOrDefaultAsync(pr => pr.Id == request.Id, token);

            if (purchaseRequest is null)
                return Result.Failure(PurchaseRequestErrors.NotFound);

            if (purchaseRequest.Status != PurchaseRequestStatus.Draft)
                return Result.Failure(PurchaseRequestErrors.CannotDelete);

            dbContext.PurchaseRequests.Remove(purchaseRequest);
            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
