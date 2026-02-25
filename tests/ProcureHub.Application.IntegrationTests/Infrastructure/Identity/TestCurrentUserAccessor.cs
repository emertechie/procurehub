using System.Collections.Immutable;

namespace ProcureHub.Application.IntegrationTests.Infrastructure.Identity;

public sealed class TestCurrentUserAccessor
{
    private static readonly AsyncLocal<TestCurrentUserContext?> Current = new();

    public IDisposable SetAuthenticatedUser(Guid userId, params string[] roles)
    {
        var previous = Current.Value;
        Current.Value = new TestCurrentUserContext(userId, roles);

        return new RestoreScope(previous);
    }

    public IDisposable SetAnonymousUser()
    {
        var previous = Current.Value;
        Current.Value = TestCurrentUserContext.Anonymous;

        return new RestoreScope(previous);
    }

    public TestCurrentUserContext GetCurrent()
    {
        return Current.Value ?? TestCurrentUserContext.Anonymous;
    }

    public void Clear()
    {
        Current.Value = null;
    }

    private sealed class RestoreScope(TestCurrentUserContext? previous) : IDisposable
    {
        public void Dispose()
        {
            Current.Value = previous;
        }
    }
}

public sealed record TestCurrentUserContext(Guid? UserId, ImmutableHashSet<string> Roles)
{
    public static readonly TestCurrentUserContext Anonymous = new(null, ImmutableHashSet<string>.Empty);

    public TestCurrentUserContext(Guid userId, IEnumerable<string> roles)
        : this(
            (Guid?)userId,
            roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase))
    {
    }
}
