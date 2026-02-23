using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Features.PurchaseRequests.Errors;
using ProcureHub.Domain.Common;

namespace ProcureHub.Application.Features.PurchaseRequests;

public static class ApprovePurchaseRequest
{
    public record Command(Guid Id, string ReviewerUserId);

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.ReviewerUserId).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(command);
            var purchaseRequest = await dbContext.PurchaseRequests
                .FirstOrDefaultAsync(pr => pr.Id == command.Id, token);

            if (purchaseRequest is null)
            {
                return Result.Failure(PurchaseRequestErrors.NotFound);
            }

            var result = purchaseRequest.Approve(command.ReviewerUserId);
            if (result.IsFailure)
            {
                return result;
            }

            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
