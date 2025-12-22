namespace ProcureHub.WebApi.Tests.Infrastructure;

// ReSharper disable once ClassNeverInstantiated.Global
public class IntegrationTestFixture : IDisposable
{
    public IntegrationTestFixture()
    {
        Console.WriteLine("*** In IntegrationTestFixture ctor");

        var connectionString = Configuration.GetConnectionString();
        WebApiTestHost = new WebApiTestHost(connectionString);
    }

    public WebApiTestHost WebApiTestHost { get; } = null!;

    public void Dispose()
    {
        Console.WriteLine("*** In IntegrationTestFixture.Dispose");
        WebApiTestHost?.Dispose();
    }
}
