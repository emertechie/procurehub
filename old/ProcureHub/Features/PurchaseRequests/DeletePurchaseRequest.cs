using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Common;
using ProcureHub.Features.PurchaseRequests.Validation;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.PurchaseRequests;

public static class DeletePurchaseRequest
{
    public record Command(Guid Id);

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
        }
    }

    public class Handler(ApplicationDbContext dbContext) : ICommandHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken token)
        {
            var purchaseRequest = await dbContext.PurchaseRequests
                .FirstOrDefaultAsync(pr => pr.Id == command.Id, token);

            if (purchaseRequest is null)
            {
                return Result.Failure(PurchaseRequestErrors.NotFound);
            }

            var result = purchaseRequest.CanDelete();
            if (result.IsFailure)
            {
                return result;
            }

            dbContext.PurchaseRequests.Remove(purchaseRequest);
            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
