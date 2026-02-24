using ProcureHub.Application.IntegrationTests.Infrastructure.xUnit;
using Xunit;

namespace ProcureHub.Application.IntegrationTests.Infrastructure.BaseTestTypes;

/// <summary>
/// Automatically resets the database before each test.
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

    public async ValueTask InitializeAsync()
    {
        await DatabaseResetter.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
