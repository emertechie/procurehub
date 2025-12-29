using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProcureHub.Models;

public class User : IdentityUser
{
    public const int FirstNameMaxLength = 200;
    public const int LastNameMaxLength = 200;

    public virtual ICollection<UserRole>? UserRoles { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public Guid? DepartmentId { get; set; }

    public Department? Department { get; set; }

    public DateTime? EnabledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(User.FirstNameMaxLength);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(User.LastNameMaxLength);

        // Each User can have many entries in the UserRole join table
        builder.HasMany(e => e.UserRoles)
            .WithOne(e => e.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(s => s.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        ;
    }
}
