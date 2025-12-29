using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProcureHub.Models;

public class PurchaseRequest
{
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
}

public enum PurchaseRequestStatus
{
    Draft,
    Pending,
    Approved,
    Rejected
}

public class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public const int RequestNumberMaxLength = 50;
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int BusinessJustificationMaxLength = 1000;

    public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.ToTable("PurchaseRequests");

        builder.Property(pr => pr.Id)
            .HasDefaultValueSql("uuidv7()");

        builder.Property(pr => pr.RequestNumber)
            .IsRequired()
            .HasMaxLength(RequestNumberMaxLength);

        builder.HasIndex(pr => pr.RequestNumber)
            .IsUnique();

        builder.Property(pr => pr.Title)
            .IsRequired()
            .HasMaxLength(TitleMaxLength);

        builder.Property(pr => pr.Description)
            .HasMaxLength(DescriptionMaxLength);

        builder.Property(pr => pr.EstimatedAmount)
            .HasPrecision(18, 2);

        builder.Property(pr => pr.BusinessJustification)
            .HasMaxLength(BusinessJustificationMaxLength);

        builder.Property(pr => pr.Status)
            .IsRequired()
            .HasConversion<string>();

        // Relationships
        builder.HasOne(pr => pr.Category)
            .WithMany()
            .HasForeignKey(pr => pr.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Department)
            .WithMany()
            .HasForeignKey(pr => pr.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Requester)
            .WithMany()
            .HasForeignKey(pr => pr.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.ReviewedBy)
            .WithMany()
            .HasForeignKey(pr => pr.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
