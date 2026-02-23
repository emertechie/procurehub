using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Features.PurchaseRequests.Errors;
using ProcureHub.Application.Features.PurchaseRequests.Services;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.PurchaseRequests;

public static class CreatePurchaseRequest
{
    public record Command(
        string Title,
        string? Description,
        decimal EstimatedAmount,
        string? BusinessJustification,
        Guid CategoryId,
        Guid DepartmentId,
        string RequesterUserId
    );

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Title).NotEmpty().MaximumLength(PurchaseRequest.TitleMaxLength);
            RuleFor(r => r.Description).MaximumLength(PurchaseRequest.DescriptionMaxLength);
            RuleFor(r => r.EstimatedAmount).GreaterThan(0);
            RuleFor(r => r.BusinessJustification).MaximumLength(PurchaseRequest.BusinessJustificationMaxLength);
            RuleFor(r => r.CategoryId).NotEmpty();
            RuleFor(r => r.DepartmentId).NotEmpty();
            RuleFor(r => r.RequesterUserId).NotEmpty();
        }
    }

    public class Handler(
        IApplicationDbContext dbContext,
        IDbConstraints dbConstraints,
        PurchaseRequestNumberGenerator purchaseRequestNumberGenerator)
        : ICommandHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> HandleAsync(Command command, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(command);
            var requestNumber = await purchaseRequestNumberGenerator.GenerateAsync(token);

            var now = DateTime.UtcNow;
            var purchaseRequest = new PurchaseRequest
            {
                Title = command.Title,
                Description = command.Description,
                EstimatedAmount = command.EstimatedAmount,
                BusinessJustification = command.BusinessJustification,
                CategoryId = command.CategoryId,
                DepartmentId = command.DepartmentId,
                RequesterId = command.RequesterUserId,
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

            return Result.Success(purchaseRequest.Id);
        }
    }
}
