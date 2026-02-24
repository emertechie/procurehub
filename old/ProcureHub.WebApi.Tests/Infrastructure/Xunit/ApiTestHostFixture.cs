namespace ProcureHub.WebApi.Tests.Infrastructure.Xunit;

public class ApiTestHostFixture : IDisposable
{
    public ApiTestHostFixture()
    {
        var connectionString = Configuration.GetConnectionString();
        ApiTestHost = new ApiTestHost(connectionString);
    }

    public ApiTestHost ApiTestHost { get; } = null!;

    public void Dispose()
    {
        ApiTestHost?.Dispose();
    }
}
