namespace ProcureHub.Application.Abstractions.Identity;

public interface ICurrentUserProvider
{
    Task<ICurrentUser> GetCurrentUserAsync();
}
