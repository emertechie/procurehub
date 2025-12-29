using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.PurchaseRequests;

public static class RejectPurchaseRequest
{
    public record Request(Guid Id, string UserId);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var purchaseRequest = await dbContext.PurchaseRequests
                .FirstOrDefaultAsync(pr => pr.Id == request.Id, token);

            if (purchaseRequest is null)
                return Result.Failure(PurchaseRequestErrors.NotFound);

            if (purchaseRequest.Status != PurchaseRequestStatus.Pending)
                return Result.Failure(PurchaseRequestErrors.NotPending);

            purchaseRequest.Status = PurchaseRequestStatus.Rejected;
            purchaseRequest.ReviewedAt = DateTime.UtcNow;
            purchaseRequest.ReviewedById = request.UserId;
            purchaseRequest.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
