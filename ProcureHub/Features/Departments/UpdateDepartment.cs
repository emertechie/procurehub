using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.Departments.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Departments;

public static class UpdateDepartment
{
    public record Request(Guid Id, string Name);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Name).NotEmpty().MaximumLength(DepartmentConfiguration.NameMaxLength);
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

            try
            {
                await dbContext.SaveChangesAsync(token);
                return Result.Success();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation("IX_Departments_Name"))
            {
                return Result.Failure(DepartmentErrors.DuplicateName(request.Name));
            }
        }
    }
}
