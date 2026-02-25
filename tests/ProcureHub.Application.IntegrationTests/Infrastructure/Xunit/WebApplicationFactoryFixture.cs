namespace ProcureHub.Application.IntegrationTests.Infrastructure.Xunit;

public sealed class WebApplicationFactoryFixture : IDisposable
{
    public WebApplicationFactoryFixture()
    {
        var connectionString = Configuration.GetConnectionString();
        WebApplicationFactory = new CustomWebApplicationFactory(connectionString);
    }

    public CustomWebApplicationFactory WebApplicationFactory { get; } = null!;

    public void Dispose()
    {
        WebApplicationFactory?.Dispose();
    }
}
