using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.PurchaseRequests;

public static class SubmitPurchaseRequest
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
                return Result.Failure(PurchaseRequestErrors.CannotSubmitNonDraft);

            purchaseRequest.Status = PurchaseRequestStatus.Pending;
            purchaseRequest.SubmittedAt = DateTime.UtcNow;
            purchaseRequest.UpdatedAt = DateTime.UtcNow;

            // Generate request number if not already set
            if (string.IsNullOrEmpty(purchaseRequest.RequestNumber))
            {
                var year = DateTime.UtcNow.Year;
                var count = await dbContext.PurchaseRequests
                    .CountAsync(pr => pr.RequestNumber.StartsWith($"PR-{year}-"), token);
                purchaseRequest.RequestNumber = $"PR-{year}-{(count + 1):D3}";
            }

            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
