using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProcureHub.Models;

public class Department
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public ICollection<User> Users { get; set; } = new HashSet<User>();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public const int NameMaxLength = 200;

    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.Property(d => d.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength);

        builder.HasIndex(d => d.Name)
            .IsUnique();
    }
}
