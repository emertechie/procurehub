using ProcureHub.Models;

namespace ProcureHub.Features.Users.Validation;

public class UserSigninValidator
{
#pragma warning disable CA1822
    public Task<bool> CanSignInAsync(User user)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!user.EnabledAt.HasValue)
        {
            // TODO: set up EventId and log
            // Logger.LogDebug(EventIds.DisabledUserCannotSignIn, "Disabled user cannot sign in.");

            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
