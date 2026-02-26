using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Common.Authorization;

namespace ProcureHub.Application.Features.Departments;

public static class GetDepartmentById
{
    [AuthorizeRequest]
    public record Request(Guid id) : IRequest<Response?>;

    public record Response(Guid Id, string Name);

    public class Handler(IApplicationDbContext dbContext)
        : IRequestHandler<Request, Response?>
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
