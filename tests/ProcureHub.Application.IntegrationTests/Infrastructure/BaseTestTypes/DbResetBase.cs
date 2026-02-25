using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Application.IntegrationTests.Infrastructure.Xunit;
using ProcureHub.Application.Common;
using ProcureHub.Application.Constants;
using ProcureHub.Application.IntegrationTests.Infrastructure.Identity;
using Xunit;

namespace ProcureHub.Application.IntegrationTests.Infrastructure.BaseTestTypes;

/// <summary>
/// Automatically resets the database before each test and ensures roles and an admin user is set up.
/// </summary>
public abstract class DbResetBase : IAsyncLifetime
{
    protected DbResetBase(
        WebApplicationFactoryFixture webApplicationFactoryFixture,
        ITestOutputHelper testOutputHelper)
    {
        WebApplicationFactory = webApplicationFactoryFixture.WebApplicationFactory;
        WebApplicationFactory.OutputHelper = testOutputHelper;
    }
    
    protected CustomWebApplicationFactory WebApplicationFactory { get; private set; }

    protected IServiceScope CreateScope()
    {
        return WebApplicationFactory.Services.CreateScope();
    }

    protected async Task<TResponse> ExecuteQueryAsync<TRequest, TResponse>(TRequest request)
    {
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TRequest, TResponse>>();
        return await handler.HandleAsync(request, CancellationToken.None);
    }

    protected async Task<TResponse> ExecuteCommandAsync<TCommand, TResponse>(TCommand command)
    {
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        return await handler.HandleAsync(command, CancellationToken.None);
    }

    protected IDisposable RunAs(Guid userId, params string[] roles)
    {
        var accessor = WebApplicationFactory.Services.GetRequiredService<TestCurrentUserAccessor>();
        return accessor.SetAuthenticatedUser(userId, roles);
    }

    protected IDisposable RunAsAdmin(Guid userId)
    {
        return RunAs(userId, RoleNames.Admin);
    }

    public async ValueTask InitializeAsync()
    {
        await DatabaseResetter.ResetDatabaseAsync();

        using var scope = CreateScope();
        var identityData = scope.ServiceProvider.GetRequiredService<IdentityTestDataBuilder>();
        await identityData.EnsureRolesAsync();
        await identityData.EnsureAdminAsync();

        var currentUserAccessor = WebApplicationFactory.Services.GetRequiredService<TestCurrentUserAccessor>();
        currentUserAccessor.Clear();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
