using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Common.Authorization;
using ProcureHub.Domain.Common;

namespace ProcureHub.Application.Features.Categories;

public static class GetCategoryById
{
    [AuthorizeRequest]
    public record Request(Guid Id) : IRequest<Result<Response>>;

    public record Response(Guid Id, string Name, DateTime CreatedAt, DateTime UpdatedAt);

    public class Handler(IApplicationDbContext dbContext)
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
