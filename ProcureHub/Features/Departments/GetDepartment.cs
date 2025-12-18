using Microsoft.EntityFrameworkCore;

using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Departments;

public static class GetDepartment
{
    public record Request(int id);

    public record Response(int Id, string Name);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Response?>
    {
        public Task<Response?> HandleAsync(Request request, CancellationToken token)
        {
            return dbContext.Departments
                .AsNoTracking()
                .Where(d => d.Id == request.id)
                .Select(d => new Response(d.Id, d.Name!))
                .FirstOrDefaultAsync(token);
        }
    }
}