using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Categories;

public static class GetCategoryById
{
    public record Request(Guid Id);

    public record Response(Guid Id, string Name, DateTime CreatedAt, DateTime UpdatedAt);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Result<Response>>
    {
        public async Task<Result<Response>> HandleAsync(Request request, CancellationToken token)
        {
            var category = await dbContext.Categories
                .AsNoTracking()
                .Where(c => c.Id == request.Id)
                .Select(c => new Response(c.Id, c.Name, c.CreatedAt, c.UpdatedAt))
                .FirstOrDefaultAsync(token);

            return category is null
                ? Result.Failure<Response>(Error.NotFound("Category not found"))
                : Result.Success(category);
        }
    }
}
