using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.Categories.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Categories;

public static class UpdateCategory
{
    public record Request(Guid Id, string Name);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Name).NotEmpty().MaximumLength(CategoryConfiguration.NameMaxLength);
        }
    }

    public class Handler(ApplicationDbContext dbContext)
        : ICommandHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var category = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id, token);

            if (category is null)
            {
                return Result.Failure(Error.NotFound("Category not found"));
            }

            category.Name = request.Name;
            category.UpdatedAt = DateTime.UtcNow;

            try
            {
                await dbContext.SaveChangesAsync(token);
                return Result.Success();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation("IX_Categories_Name"))
            {
                return Result.Failure(CategoryErrors.DuplicateName(request.Name));
            }
        }
    }
}
