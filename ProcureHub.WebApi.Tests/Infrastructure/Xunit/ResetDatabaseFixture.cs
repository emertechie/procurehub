using ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;

namespace ProcureHub.WebApi.Tests.Infrastructure.Xunit;

public class ResetDatabaseFixture(ApiTestHostFixture hostFixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        await DatabaseResetter.ResetDatabaseAsync();
        await DatabaseResetter.SeedDataAsync(hostFixture.ApiTestHost.Services, HttpClientBase.AdminEmail, HttpClientBase.AdminPassword);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
