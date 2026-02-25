using ProcureHub.Application.Abstractions.Identity;

namespace ProcureHub.Application.IntegrationTests.Infrastructure.Identity;

public sealed class TestCurrentUserProvider(TestCurrentUserAccessor accessor)
    : ICurrentUserProvider
{
    public Task<ICurrentUser> GetCurrentUserAsync()
    {
        var context = accessor.GetCurrent();
        ICurrentUser currentUser = new TestCurrentUser(context);
        return Task.FromResult(currentUser);
    }
}

internal sealed class TestCurrentUser(TestCurrentUserContext context) : ICurrentUser
{
    public Guid? UserId => context.UserId;

    public bool IsInRole(string roleName)
    {
        return context.Roles.Contains(roleName);
    }
}
