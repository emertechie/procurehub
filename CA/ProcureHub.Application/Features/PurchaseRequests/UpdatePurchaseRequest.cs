using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Features.PurchaseRequests.Errors;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.PurchaseRequests;

public static class UpdatePurchaseRequest
{
    public record Command(
        Guid Id,
        string Title,
        string? Description,
        decimal EstimatedAmount,
        string? BusinessJustification,
        Guid CategoryId,
        Guid DepartmentId
    );

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Title).NotEmpty().MaximumLength(PurchaseRequest.TitleMaxLength);
            RuleFor(r => r.Description).MaximumLength(PurchaseRequest.DescriptionMaxLength);
            RuleFor(r => r.EstimatedAmount).GreaterThan(0);
            RuleFor(r => r.BusinessJustification).MaximumLength(PurchaseRequest.BusinessJustificationMaxLength);
            RuleFor(r => r.CategoryId).NotEmpty();
            RuleFor(r => r.DepartmentId).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext dbContext, IDbConstraints dbConstraints)
        : ICommandHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            var purchaseRequest = await dbContext.PurchaseRequests
                .FirstOrDefaultAsync(pr => pr.Id == command.Id, cancellationToken);

            if (purchaseRequest is null)
            {
                return Result.Failure(PurchaseRequestAppErrors.NotFound);
            }

            var result = purchaseRequest.CanUpdate();
            if (result.IsFailure)
            {
                return result;
            }

            purchaseRequest.Title = command.Title;
            purchaseRequest.Description = command.Description;
            purchaseRequest.EstimatedAmount = command.EstimatedAmount;
            purchaseRequest.BusinessJustification = command.BusinessJustification;
            purchaseRequest.CategoryId = command.CategoryId;
            purchaseRequest.DepartmentId = command.DepartmentId;
            purchaseRequest.UpdatedAt = DateTime.UtcNow;

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (
                dbConstraints.IsForeignKeyViolation(ex, nameof(PurchaseRequest), nameof(Category), nameof(Category.Id)))
            {
                return Result.Failure<Guid>(PurchaseRequestAppErrors.CategoryNotFound);
            }
            catch (DbUpdateException ex) when (
                dbConstraints.IsForeignKeyViolation(ex, nameof(PurchaseRequest), nameof(PurchaseRequest.Department), nameof(Department.Id)))
            {
                return Result.Failure<Guid>(PurchaseRequestAppErrors.DepartmentNotFound);
            }

            return Result.Success();
        }
    }
}
