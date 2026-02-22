using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Infrastructure.Database.Configurations;

internal sealed class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public const int RequestNumberMaxLength = 50;
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int BusinessJustificationMaxLength = 1000;

    public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PurchaseRequests");

        builder.Property(pr => pr.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

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
