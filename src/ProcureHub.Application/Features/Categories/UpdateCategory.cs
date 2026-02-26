using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Common.Authorization;
using ProcureHub.Application.Constants;
using ProcureHub.Application.Features.Categories.Validation;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.Categories;

public static class UpdateCategory
{
    [AuthorizeRequest(RoleNames.Admin)]
    public record Command(Guid Id, string Name) : IRequest<Result>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Name).NotEmpty().MaximumLength(Category.NameMaxLength);
        }
    }

    public class Handler(IApplicationDbContext dbContext, IDbConstraints dbConstraints)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            var category = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

            if (category is null)
            {
                return Result.Failure(Error.NotFound("Category not found"));
            }

            category.Name = command.Name;
            category.UpdatedAt = DateTime.UtcNow;

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateException ex) when (
                dbConstraints.IsUniqueConstraintViolation(ex, nameof(Category), nameof(Category.Name)))
            {
                return Result.Failure(CategoryErrors.DuplicateName(command.Name));
            }
        }
    }
}
