using ProcureHub.Domain.Common;
using ProcureHub.Domain.Entities.Errors;

namespace ProcureHub.Domain.Entities;

public enum PurchaseRequestStatus
{
    Draft,
    Pending,
    Approved,
    Rejected
}

public class PurchaseRequest
{
    public const int RequestNumberMaxLength = 50;
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int BusinessJustificationMaxLength = 1000;

    public Guid Id { get; init; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal EstimatedAmount { get; set; }
    public string? BusinessJustification { get; set; }
    public Guid CategoryId { get; set; }
    public Guid DepartmentId { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    public PurchaseRequestStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedById { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public User Requester { get; set; } = null!;
    public User? ReviewedBy { get; set; }

    // Domain logic methods

    public Result Submit()
    {
        if (Status != PurchaseRequestStatus.Draft)
        {
            return Result.Failure(PurchaseRequestDomainErrors.CannotSubmitNonDraft);
        }

        Status = PurchaseRequestStatus.Pending;

        var now = DateTime.UtcNow;
        SubmittedAt = now;
        UpdatedAt = now;

        return Result.Success();
    }

    public Result Approve(string reviewerUserId)
    {
        if (Status != PurchaseRequestStatus.Pending)
        {
            return Result.Failure(PurchaseRequestDomainErrors.CannotApproveNonPending);
        }

        if (reviewerUserId == RequesterId)
        {
            return Result.Failure(PurchaseRequestDomainErrors.CannotApproveOwnRequest);
        }

        Status = PurchaseRequestStatus.Approved;
        ReviewedById = reviewerUserId;

        var now = DateTime.UtcNow;
        ReviewedAt = now;
        UpdatedAt = now;

        return Result.Success();
    }

    public Result Reject(string reviewerUserId)
    {
        if (Status != PurchaseRequestStatus.Pending)
        {
            return Result.Failure(PurchaseRequestDomainErrors.CannotRejectNonPending);
        }

        Status = PurchaseRequestStatus.Rejected;
        ReviewedById = reviewerUserId;

        var now = DateTime.UtcNow;
        ReviewedAt = now;
        UpdatedAt = now;

        return Result.Success();
    }

    public Result CanUpdate()
    {
        return Status == PurchaseRequestStatus.Draft
            ? Result.Success()
            : Result.Failure(PurchaseRequestDomainErrors.CannotUpdateNonDraft);
    }

    public Result CanDelete()
    {
        return Status == PurchaseRequestStatus.Draft
            ? Result.Success()
            : Result.Failure(PurchaseRequestDomainErrors.CannotDeleteNonDraft);
    }

    public Result Withdraw()
    {
        if (Status != PurchaseRequestStatus.Pending)
        {
            return Result.Failure(PurchaseRequestDomainErrors.CannotWithdrawNonPending);
        }

        Status = PurchaseRequestStatus.Draft;
        SubmittedAt = null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
