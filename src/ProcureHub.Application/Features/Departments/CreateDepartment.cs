using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Common.Authorization;
using ProcureHub.Application.Constants;
using ProcureHub.Application.Features.Departments.Validation;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.Departments;

public static class CreateDepartment
{
    [AuthorizeRequest(RoleNames.Admin)]
    public record Command(string Name) : IRequest<Result<Guid>>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Name).NotEmpty().MaximumLength(Department.NameMaxLength);
        }
    }

    public class Handler(IApplicationDbContext dbContext, IDbConstraints dbConstraints)
        : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var department = new Department { Name = command.Name };
            await dbContext.Departments.AddAsync(department, cancellationToken);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success(department.Id);
            }
            catch (DbUpdateException ex) when (
                dbConstraints.IsUniqueConstraintViolation(ex, nameof(Department), nameof(Department.Name)))
            {
                return Result.Failure<Guid>(DepartmentErrors.DuplicateName(command.Name));
            }
        }
    }
}
