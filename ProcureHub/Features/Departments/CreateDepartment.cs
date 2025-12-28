using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.Departments.Validation;
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
        : IRequestHandler<Request, Result<Guid>>
    {
        public async Task<Result<Guid>> HandleAsync(Request request, CancellationToken token)
        {
            var department = new Department { Name = request.Name };
            await dbContext.Departments.AddAsync(department, token);

            try
            {
                await dbContext.SaveChangesAsync(token);
                return Result.Success(department.Id);
            }
            catch (DbUpdateException ex) when (DatabaseErrors.IsUniqueConstraintViolation(ex, "IX_Departments_Name"))
            {
                return Result.Failure<Guid>(DepartmentErrors.DuplicateName(request.Name));
            }
        }
    }
}
