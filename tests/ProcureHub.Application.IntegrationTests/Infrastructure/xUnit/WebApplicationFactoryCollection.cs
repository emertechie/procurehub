using Xunit;

namespace ProcureHub.Application.IntegrationTests.Infrastructure.xUnit;

[CollectionDefinition("WebApplicationFactory")]
public class WebApplicationFactoryCollection : ICollectionFixture<WebApplicationFactoryFixture>
{
}
