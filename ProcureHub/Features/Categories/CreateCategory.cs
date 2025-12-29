using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.Categories.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Categories;

public static class CreateCategory
{
    public record Request(string Name);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Name).NotEmpty().MaximumLength(Category.NameMaxLength);
        }
    }

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Result<Guid>>
    {
        public async Task<Result<Guid>> HandleAsync(Request request, CancellationToken token)
        {
            var now = DateTime.UtcNow;
            var category = new Category
            {
                Name = request.Name,
                CreatedAt = now,
                UpdatedAt = now
            };

            await dbContext.Categories.AddAsync(category, token);

            try
            {
                await dbContext.SaveChangesAsync(token);
                return Result.Success(category.Id);
            }
            catch (DbUpdateException ex) when (DatabaseException.IsUniqueConstraintViolation(ex, "IX_Categories_Name"))
            {
                return Result.Failure<Guid>(CategoryErrors.DuplicateName(request.Name));
            }
        }
    }
}
