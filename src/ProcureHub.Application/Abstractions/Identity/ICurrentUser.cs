namespace ProcureHub.Application.Abstractions.Identity;

public interface ICurrentUser
{
    // TODO: this should probably be a string
    Guid? UserId { get; }

    bool IsInRole(string roleName);
}
