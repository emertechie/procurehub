using ProcureHub.Models;

namespace ProcureHub.Features.Users;

public class UserSigninValidator
{
    public Task<bool> CanSignInAsync(User user)
    {
        if (!user.EnabledAt.HasValue)
        {
            // TODO: set up EventId and log
            // Logger.LogDebug(EventIds.DisabledUserCannotSignIn, "Disabled user cannot sign in.");

            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
