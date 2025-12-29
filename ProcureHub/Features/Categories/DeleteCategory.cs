using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Categories;

public static class DeleteCategory
{
    public record Request(Guid Id);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var category = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id, token);

            if (category is null)
            {
                return Result.Failure(Error.NotFound("Category not found"));
            }

            // TODO: Check if category has purchase requests before deleting
            // Uncomment once PurchaseRequest model is implemented:
            // var requestsForCategory = await dbContext.PurchaseRequests
            //     .CountAsync(pr => pr.CategoryId == request.Id, token);
            //
            // if (requestsForCategory > 0)
            // {
            //     return Result.Failure(Error.Validation(
            //         $"Cannot delete category. It has {requestsForCategory} purchase request(s). Please reassign requests before deleting."));
            // }

            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
