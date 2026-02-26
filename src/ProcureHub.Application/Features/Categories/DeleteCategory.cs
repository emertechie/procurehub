using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Common.Authorization;
using ProcureHub.Application.Constants;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.Categories;

public static class DeleteCategory
{
    [AuthorizeRequest(RoleNames.Admin)]
    public record Command(Guid Id) : IRequest<Result>;

    public class Handler(IApplicationDbContext dbContext, IDbConstraints dbConstraints)
        : IRequestHandler<Command, Result>
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
            catch (DbUpdateException ex) when (
                dbConstraints.IsForeignKeyRestrictViolation(ex, nameof(PurchaseRequest), nameof(PurchaseRequest.CategoryId)))
            {
                return Result.Failure(Error.Validation(
                    $"Cannot delete category. It has one or more purchase requests. Please reassign requests before deleting."));
            }

            return Result.Success();
        }
    }
}
