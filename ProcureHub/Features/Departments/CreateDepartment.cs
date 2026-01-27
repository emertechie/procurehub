using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.Departments.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Departments;

public static class CreateDepartment
{
    public record Request(string Name);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Name).NotEmpty().MaximumLength(DepartmentConfiguration.NameMaxLength);
        }
    }

    public class Handler(ApplicationDbContext dbContext)
        : ICommandHandler<Request, Result<Guid>>
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
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation("IX_Departments_Name"))
            {
                return Result.Failure<Guid>(DepartmentErrors.DuplicateName(request.Name));
            }
        }
    }
}
