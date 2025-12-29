using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.PurchaseRequests.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.PurchaseRequests;

public static class UpdatePurchaseRequest
{
    public record Request(
        Guid Id,
        string Title,
        string? Description,
        decimal EstimatedAmount,
        string? BusinessJustification,
        Guid CategoryId,
        Guid DepartmentId
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

    public class Handler(ApplicationDbContext dbContext) : IRequestHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var purchaseRequest = await dbContext.PurchaseRequests
                .FirstOrDefaultAsync(pr => pr.Id == request.Id, token);

            if (purchaseRequest is null)
                return Result.Failure(PurchaseRequestErrors.NotFound);

            if (purchaseRequest.Status != PurchaseRequestStatus.Draft)
                return Result.Failure(PurchaseRequestErrors.CannotUpdateNonDraft);

            // Verify category exists
            var categoryExists = await dbContext.Categories
                .AnyAsync(c => c.Id == request.CategoryId, token);
            if (!categoryExists)
                return Result.Failure(PurchaseRequestErrors.CategoryNotFound);

            // Verify department exists
            var departmentExists = await dbContext.Departments
                .AnyAsync(d => d.Id == request.DepartmentId, token);
            if (!departmentExists)
                return Result.Failure(PurchaseRequestErrors.DepartmentNotFound);

            purchaseRequest.Title = request.Title;
            purchaseRequest.Description = request.Description;
            purchaseRequest.EstimatedAmount = request.EstimatedAmount;
            purchaseRequest.BusinessJustification = request.BusinessJustification;
            purchaseRequest.CategoryId = request.CategoryId;
            purchaseRequest.DepartmentId = request.DepartmentId;
            purchaseRequest.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
