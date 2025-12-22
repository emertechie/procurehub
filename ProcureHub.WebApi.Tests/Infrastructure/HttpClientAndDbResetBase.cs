namespace ProcureHub.WebApi.Tests.Infrastructure;

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
        Console.WriteLine("*** In IntegrationTestsBase.InitializeAsync. Resetting database");

        await DatabaseResetter.ResetDatabaseAsync();
        await DatabaseResetter.SeedDataAsync(ApiTestHost.Services, AdminEmail, AdminPassword);
    }

    public ValueTask DisposeAsync()
    {
        Console.WriteLine("*** In IntegrationTestsBase.DisposeAsync");
        return ValueTask.CompletedTask;
    }
}
