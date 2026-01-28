using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Categories;

public static class DeleteCategory
{
    public record Command(Guid Id);

    public class Handler(ApplicationDbContext dbContext)
        : ICommandHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken token)
        {
            var category = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == command.Id, token);

            if (category is null)
            {
                return Result.Failure(Error.NotFound("Category not found"));
            }

            dbContext.Categories.Remove(category);

            try
            {
                await dbContext.SaveChangesAsync(token);
            }
            catch (DbUpdateException ex) when (ex.IsForeignKeyRestrictViolation("FK_PurchaseRequests_Categories_CategoryId"))
            {
                return Result.Failure(Error.Validation(
                    $"Cannot delete category. It has one or more purchase requests. Please reassign requests before deleting."));
            }

            return Result.Success();
        }
    }
}
