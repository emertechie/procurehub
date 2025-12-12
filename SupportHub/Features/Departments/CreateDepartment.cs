using SupportHub.Infrastructure;
using SupportHub.Models;

namespace SupportHub.Features.Departments;

public static class CreateDepartment
{
    public record Request(string Name);

    // TODO: validator

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, int>
    {
        public async Task<int> HandleAsync(Request request, CancellationToken token)
        {
            var department = new Department { Name = request.Name };
            var result = await dbContext.Departments.AddAsync(department, token);
            await dbContext.SaveChangesAsync(token);
            return result.Entity.Id;
        }
    }
}