namespace ProcureHub.Infrastructure.Authentication;

public interface ICurrentUser
{
    Guid? UserId { get; }

    IReadOnlyCollection<string> Roles { get; }
}
