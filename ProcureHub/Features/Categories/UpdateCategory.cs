using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.Categories.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Categories;

public static class UpdateCategory
{
    public record Command(Guid Id, string Name);

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Name).NotEmpty().MaximumLength(CategoryConfiguration.NameMaxLength);
        }
    }

    public class Handler(ApplicationDbContext dbContext)
        : ICommandHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(command);
            var category = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == command.Id, token);

            if (category is null)
            {
                return Result.Failure(Error.NotFound("Category not found"));
            }

            category.Name = command.Name;
            category.UpdatedAt = DateTime.UtcNow;

            try
            {
                await dbContext.SaveChangesAsync(token);
                return Result.Success();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation("IX_Categories_Name"))
            {
                return Result.Failure(CategoryErrors.DuplicateName(command.Name));
            }
        }
    }
}
