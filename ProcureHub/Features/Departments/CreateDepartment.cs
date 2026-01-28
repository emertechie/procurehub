using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.Departments.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Departments;

public static class CreateDepartment
{
    public record Command(string Name);

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Name).NotEmpty().MaximumLength(DepartmentConfiguration.NameMaxLength);
        }
    }

    public class Handler(ApplicationDbContext dbContext)
        : ICommandHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> HandleAsync(Command command, CancellationToken token)
        {
            var department = new Department { Name = command.Name };
            await dbContext.Departments.AddAsync(department, token);

            try
            {
                await dbContext.SaveChangesAsync(token);
                return Result.Success(department.Id);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation("IX_Departments_Name"))
            {
                return Result.Failure<Guid>(DepartmentErrors.DuplicateName(command.Name));
            }
        }
    }
}
