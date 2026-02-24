using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;

namespace ProcureHub.Application.Features.PurchaseRequests.Services;

public class PurchaseRequestNumberGenerator(IApplicationDbContext dbContext)
{
    /// <summary>
    /// Generates a new unique purchase request number in the format "PR-YYYY-XXX".
    /// Quick and dirty implementation for demo. Better to use DB generator.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<string> GenerateAsync(CancellationToken token)
    {
        var year = DateTime.UtcNow.Year;
        var yearPrefix = $"PR-{year}-";

        var count = await dbContext.PurchaseRequests
            .CountAsync(pr => pr.RequestNumber.StartsWith(yearPrefix), token);

        return $"{yearPrefix}{(count + 1):D3}";
    }
}
