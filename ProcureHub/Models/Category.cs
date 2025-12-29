using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProcureHub.Models;

public class Category
{
    public const int NameMaxLength = 100;

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.Property(d => d.Id)
            .HasDefaultValueSql("uuidv7()");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(Category.NameMaxLength);

        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();
    }
}
