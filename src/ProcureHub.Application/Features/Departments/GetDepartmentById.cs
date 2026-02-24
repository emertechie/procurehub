using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;

namespace ProcureHub.Application.Features.Departments;

public static class GetDepartmentById
{
    public record Request(Guid id);

    public record Response(Guid Id, string Name);

    public class Handler(IApplicationDbContext dbContext)
        : IQueryHandler<Request, Response?>
    {
        public Task<Response?> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            return dbContext.Departments
                .AsNoTracking()
                .Where(d => d.Id == request.id)
                .Select(d => new Response(d.Id, d.Name!))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
