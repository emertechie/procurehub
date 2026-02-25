using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Application.Common;
using ProcureHub.Application.Features.PurchaseRequests.Errors;
using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.PurchaseRequests;

public static class RejectPurchaseRequest
{
    public record Command(Guid Id, string ReviewerUserId) : IRequest<Result>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.ReviewerUserId).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext dbContext)
        : IRequestHandler<Command, Result>
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

            var result = purchaseRequest.Reject(command.ReviewerUserId);
            if (result.IsFailure)
            {
                return result;
            }

            await dbContext.SaveChangesAsync(token);

            return Result.Success();
        }
    }
}
