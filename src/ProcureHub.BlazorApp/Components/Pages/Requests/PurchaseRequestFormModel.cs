using FluentValidation;
using ProcureHub.Domain.Entities;

namespace ProcureHub.BlazorApp.Components.Pages.Requests;

public class PurchaseRequestFormModel
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public decimal EstimatedAmount { get; set; }
    public string? BusinessJustification { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class PurchaseRequestFormModelValidator : AbstractValidator<PurchaseRequestFormModel>
{
    public PurchaseRequestFormModelValidator()
    {
        RuleFor(m => m.Title)
            .NotEmpty()
            .MaximumLength(PurchaseRequest.TitleMaxLength);

        RuleFor(m => m.Description)
            .MaximumLength(PurchaseRequest.DescriptionMaxLength);

        RuleFor(m => m.EstimatedAmount)
            .GreaterThan(0);

        RuleFor(m => m.BusinessJustification)
            .MaximumLength(PurchaseRequest.BusinessJustificationMaxLength);

        RuleFor(m => m.CategoryId)
            .NotEmpty().WithMessage("'Category' must not be empty.");

        RuleFor(m => m.DepartmentId)
            .NotEmpty().WithMessage("'Department' must not be empty.");
    }
}
