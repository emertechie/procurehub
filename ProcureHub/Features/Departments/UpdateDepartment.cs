using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.Departments.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Departments;

public static class UpdateDepartment
{
    public record Command(Guid Id, string Name);

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Name).NotEmpty().MaximumLength(DepartmentConfiguration.NameMaxLength);
        }
    }

    public class Handler(ApplicationDbContext dbContext)
        : ICommandHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken token)
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == command.Id, token);

            if (department is null)
                return Result.Failure(Error.NotFound("Department not found"));

            department.Name = command.Name;

            try
            {
                await dbContext.SaveChangesAsync(token);
                return Result.Success();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation("IX_Departments_Name"))
            {
                return Result.Failure(DepartmentErrors.DuplicateName(command.Name));
            }
        }
    }
}
