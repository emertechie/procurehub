using Microsoft.EntityFrameworkCore;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Categories;

public static class QueryCategories
{
    public record Request();

    public record Response(Guid Id, string Name);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Response[]>
    {
        public Task<Response[]> HandleAsync(Request request, CancellationToken token)
        {
            return dbContext.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new Response(c.Id, c.Name))
                .ToArrayAsync(token);
        }
    }
}
