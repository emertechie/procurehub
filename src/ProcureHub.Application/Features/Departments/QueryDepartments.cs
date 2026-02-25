using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;

namespace ProcureHub.Application.Features.Departments;

public static class QueryDepartments
{
#pragma warning disable S2094
    public record Request() : IRequest<Response[]>;
#pragma warning restore S2094

    public record Response(Guid Id, string Name);

    public class Handler(IApplicationDbContext dbContext)
        : IRequestHandler<Request, Response[]>
    {
        public Task<Response[]> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            return dbContext.Departments
                .AsNoTracking()
                .Select(d => new Response(d.Id, d.Name!))
                .ToArrayAsync(cancellationToken);
        }
    }
}
