using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Departments;

public static class UpdateDepartment
{
    public record Request(int Id, string Name);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == request.Id, token);

            if (department is null)
                return Result.Failure(Error.NotFound("Department not found"));

            department.Name = request.Name;
            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
