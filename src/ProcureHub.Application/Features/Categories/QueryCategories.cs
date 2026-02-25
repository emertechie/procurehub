using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;

namespace ProcureHub.Application.Features.Categories;

public static class QueryCategories
{
    public record Request() : IRequest<Response[]>;

    public record Response(Guid Id, string Name);

    public class Handler(IApplicationDbContext dbContext)
        : IRequestHandler<Request, Response[]>
    {
        public Task<Response[]> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            return dbContext.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new Response(c.Id, c.Name))
                .ToArrayAsync(cancellationToken);
        }
    }
}
