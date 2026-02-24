using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.Users.Validation;

public class UserSigninValidator
{
#pragma warning disable CA1822
#pragma warning disable S2325
    public Task<bool> CanSignInAsync(User user)
#pragma warning restore S2325
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.EnabledAt.HasValue);
    }
}
