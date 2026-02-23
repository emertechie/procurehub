using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Features.Departments.Validation;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.Departments;

public static class UpdateDepartment
{
    public record Command(Guid Id, string Name);

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Name).NotEmpty().MaximumLength(Department.NameMaxLength);
        }
    }

    public class Handler(IApplicationDbContext dbContext, IDbConstraints dbConstraints)
        : ICommandHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken);

            if (department is null)
                return Result.Failure(Error.NotFound("Department not found"));

            department.Name = command.Name;

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateException ex) when (
                dbConstraints.IsUniqueConstraintViolation(ex, nameof(Department), nameof(Department.Name)))
            {
                return Result.Failure(DepartmentErrors.DuplicateName(command.Name));
            }
        }
    }
}
