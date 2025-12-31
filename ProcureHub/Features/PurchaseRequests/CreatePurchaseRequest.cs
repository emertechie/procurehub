using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.PurchaseRequests.Validation;
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
        string RequesterUserId
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
            RuleFor(r => r.RequesterUserId).NotEmpty();
        }
    }

    public class Handler(ApplicationDbContext dbContext, PurchaseRequestNumberGenerator purchaseRequestNumberGenerator)
        : IRequestHandler<Request, Result<Guid>>
    {
        public async Task<Result<Guid>> HandleAsync(Request request, CancellationToken token)
        {
            var requestNumber = await purchaseRequestNumberGenerator.GenerateAsync(token);

            var now = DateTime.UtcNow;
            var purchaseRequest = new PurchaseRequest
            {
                Title = request.Title,
                Description = request.Description,
                EstimatedAmount = request.EstimatedAmount,
                BusinessJustification = request.BusinessJustification,
                CategoryId = request.CategoryId,
                DepartmentId = request.DepartmentId,
                RequesterId = request.RequesterUserId,
                Status = PurchaseRequestStatus.Draft,
                RequestNumber = requestNumber,
                CreatedAt = now,
                UpdatedAt = now
            };

            await dbContext.PurchaseRequests.AddAsync(purchaseRequest, token);

            try
            {
                await dbContext.SaveChangesAsync(token);
            }
            catch (DbUpdateException ex) when (ex.IsForeignKeyViolation("FK_PurchaseRequests_Categories_CategoryId"))
            {
                return Result.Failure<Guid>(PurchaseRequestErrors.CategoryNotFound);
            }
            catch (DbUpdateException ex) when (ex.IsForeignKeyViolation("FK_PurchaseRequests_Departments_DepartmentId"))
            {
                return Result.Failure<Guid>(PurchaseRequestErrors.DepartmentNotFound);
            }

            return Result.Success(purchaseRequest.Id);
        }
    }
}
