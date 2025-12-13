using Microsoft.EntityFrameworkCore;
using SupportHub.Infrastructure;

namespace SupportHub.Features.Departments;

public static class ListDepartments
{
    public record Request();

    public record Response(int Id, string Name);

    // TODO: validator

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Response[]>
    {
        public Task<Response[]> HandleAsync(Request request, CancellationToken token)
        {
            return dbContext.Departments
                .AsNoTracking()
                .Select(d => new Response(d.Id, d.Name!))
                .ToArrayAsync(token);
        }
    }
}