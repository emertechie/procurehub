using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Features.Categories.Validation;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.Categories;

public static class CreateCategory
{
    public record Command(string Name) : IRequest<Result<Guid>>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Name).NotEmpty().MaximumLength(Category.NameMaxLength);
        }
    }

    public class Handler(IApplicationDbContext dbContext, IDbConstraints dbConstraints)
        : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var now = DateTime.UtcNow;
            var category = new Category
            {
                Name = command.Name,
                CreatedAt = now,
                UpdatedAt = now
            };

            await dbContext.Categories.AddAsync(category, cancellationToken);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success(category.Id);
            }
            catch (DbUpdateException ex) when (
                dbConstraints.IsUniqueConstraintViolation(ex, nameof(Category), nameof(Category.Name)))
            {
                return Result.Failure<Guid>(CategoryErrors.DuplicateName(command.Name));
            }
        }
    }
}
