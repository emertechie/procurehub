using ProcureHub.WebApi.Tests.Infrastructure.Xunit;

namespace ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;

/// <summary>
/// Automatically resets the database before each test. Derives from HttpClientBase to provide HttpClient instance.
/// </summary>
/// <param name="hostFixture"></param>
/// <param name="testOutputHelper"></param>
public abstract class HttpClientAndDbResetBase(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    : HttpClientBase(hostFixture, testOutputHelper), IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        await DatabaseResetter.ResetDatabaseAsync();
        await DatabaseResetter.SeedDataAsync(ApiTestHost.Services);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
