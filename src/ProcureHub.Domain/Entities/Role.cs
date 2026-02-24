using Microsoft.AspNetCore.Identity;

namespace ProcureHub.Domain.Entities;

public class Role : IdentityRole
{
    public Role()
    {
    }

    public Role(string roleName) : base(roleName)
    {
    }

    public virtual ICollection<UserRole>? UserRoles { get; init; }
}
