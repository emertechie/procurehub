using FluentValidation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;
using static ProcureHub.Models.Department;

namespace ProcureHub.Features.Departments;

public static class CreateDepartment
{
    public record Request(string Name);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Name).NotEmpty().MaximumLength(NameMaxLength);
        }
    }

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Guid>
    {
        public async Task<Guid> HandleAsync(Request request, CancellationToken token)
        {
            var department = new Department { Name = request.Name };
            var result = await dbContext.Departments.AddAsync(department, token);
            await dbContext.SaveChangesAsync(token);
            return result.Entity.Id;
        }
    }
}
