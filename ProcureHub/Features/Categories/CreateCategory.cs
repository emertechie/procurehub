using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.Categories.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Categories;

public static class CreateCategory
{
    public record Command(string Name);

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Name).NotEmpty().MaximumLength(CategoryConfiguration.NameMaxLength);
        }
    }

    public class Handler(ApplicationDbContext dbContext)
        : ICommandHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> HandleAsync(Command command, CancellationToken token)
        {
            var now = DateTime.UtcNow;
            var category = new Category
            {
                Name = command.Name,
                CreatedAt = now,
                UpdatedAt = now
            };

            await dbContext.Categories.AddAsync(category, token);

            try
            {
                await dbContext.SaveChangesAsync(token);
                return Result.Success(category.Id);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation("IX_Categories_Name"))
            {
                return Result.Failure<Guid>(CategoryErrors.DuplicateName(command.Name));
            }
        }
    }
}
