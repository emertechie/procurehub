using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Infrastructure.Database.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public const int NameMaxLength = 200;

    public void Configure(EntityTypeBuilder<Department> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

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
