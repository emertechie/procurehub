namespace ProcureHub.WebApi.Tests.Infrastructure;

public class ApiTestHostFixture : IDisposable
{
    public ApiTestHostFixture()
    {
        Console.WriteLine("*** In IntegrationTestFixture ctor");

        var connectionString = Configuration.GetConnectionString();
        ApiTestHost = new ApiTestHost(connectionString);
    }

    public ApiTestHost ApiTestHost { get; } = null!;

    public void Dispose()
    {
        Console.WriteLine("*** In IntegrationTestFixture.Dispose");
        ApiTestHost?.Dispose();
    }
}
