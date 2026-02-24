namespace ProcureHub.WebApi.Tests.Infrastructure.Xunit;

public class ResetDatabaseFixture(ApiTestHostFixture hostFixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        // Accessing Services triggers the WebApplicationFactory to build and start,
        // which runs the database migrations. This must happen before ResetDatabaseAsync
        // otherwise Respawn can fail with "No tables found" (depends on test run order).
        _ = hostFixture.ApiTestHost.Services;

        await DatabaseResetter.ResetDatabaseAsync();
        await DatabaseResetter.SeedDataAsync(hostFixture.ApiTestHost.Services);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
