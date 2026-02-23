using Microsoft.AspNetCore.Identity;

namespace ProcureHub.Domain.Entities;

public class User : IdentityUser
{
    public const int FirstNameMaxLength = 200;
    public const int LastNameMaxLength = 200;

    public virtual ICollection<UserRole> UserRoles { get; init; } = [];

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public Guid? DepartmentId { get; set; }

    public Department? Department { get; set; }

    public DateTime? EnabledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public static string NormalizeEmailForDisplay(string email)
    {
#pragma warning disable CA1308
        return email.ToLowerInvariant();
#pragma warning restore CA1308
    }
}
