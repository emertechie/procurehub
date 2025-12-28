using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;
using static ProcureHub.Models.Department;

namespace ProcureHub.Features.Departments;

public static class UpdateDepartment
{
    public record Request(Guid Id, string Name);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Name).NotEmpty().MaximumLength(NameMaxLength);
        }
    }

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
