namespace ProcureHub.Infrastructure.Authentication;

public interface ICurrentUserProvider
{
    Task<ICurrentUser> GetCurrentUserAsync();
}
