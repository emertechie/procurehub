using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.PurchaseRequests;

public static class CreatePurchaseRequest
{
    public record Request(
        string Title,
        string? Description,
        decimal EstimatedAmount,
        string? BusinessJustification,
        Guid CategoryId,
        Guid DepartmentId,
        string UserId
    );

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Title).NotEmpty().MaximumLength(PurchaseRequestConfiguration.TitleMaxLength);
            RuleFor(r => r.Description).MaximumLength(PurchaseRequestConfiguration.DescriptionMaxLength);
            RuleFor(r => r.EstimatedAmount).GreaterThan(0);
            RuleFor(r => r.BusinessJustification).MaximumLength(PurchaseRequestConfiguration.BusinessJustificationMaxLength);
            RuleFor(r => r.CategoryId).NotEmpty();
            RuleFor(r => r.DepartmentId).NotEmpty();
        }
    }

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Result<Guid>>
    {
        public async Task<Result<Guid>> HandleAsync(Request request, CancellationToken token)
        {
            // Verify category exists
            var categoryExists = await dbContext.Categories
                .AnyAsync(c => c.Id == request.CategoryId, token);
            if (!categoryExists)
                return Result.Failure<Guid>(PurchaseRequestErrors.CategoryNotFound);

            // Verify department exists
            var departmentExists = await dbContext.Departments
                .AnyAsync(d => d.Id == request.DepartmentId, token);
            if (!departmentExists)
                return Result.Failure<Guid>(PurchaseRequestErrors.DepartmentNotFound);

            // Generate request number
            var year = DateTime.UtcNow.Year;
            var count = await dbContext.PurchaseRequests
                .CountAsync(pr => pr.RequestNumber.StartsWith($"PR-{year}-"), token);
            var requestNumber = $"PR-{year}-{(count + 1):D3}";

            var now = DateTime.UtcNow;
            var purchaseRequest = new PurchaseRequest
            {
                Title = request.Title,
                Description = request.Description,
                EstimatedAmount = request.EstimatedAmount,
                BusinessJustification = request.BusinessJustification,
                CategoryId = request.CategoryId,
                DepartmentId = request.DepartmentId,
                RequesterId = request.UserId,
                Status = PurchaseRequestStatus.Draft,
                RequestNumber = requestNumber,
                CreatedAt = now,
                UpdatedAt = now
            };

            await dbContext.PurchaseRequests.AddAsync(purchaseRequest, token);
            await dbContext.SaveChangesAsync(token);

            return Result.Success(purchaseRequest.Id);
        }
    }
}
