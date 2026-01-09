namespace ProcureHub.Infrastructure.Authentication;

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsInRole(string roleName);
}
