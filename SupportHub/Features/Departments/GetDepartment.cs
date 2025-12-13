using Microsoft.EntityFrameworkCore;
using SupportHub.Infrastructure;

namespace SupportHub.Features.Departments;

public static class GetDepartment
{
    public record Request(int id);

    public record Response(int Id, string Name);

    // TODO: validator

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