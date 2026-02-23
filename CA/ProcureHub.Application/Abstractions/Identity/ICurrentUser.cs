namespace ProcureHub.Application.Abstractions.Identity;

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsInRole(string roleName);
}
