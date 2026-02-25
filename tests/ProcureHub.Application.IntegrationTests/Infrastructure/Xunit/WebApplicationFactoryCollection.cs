using Xunit;

namespace ProcureHub.Application.IntegrationTests.Infrastructure.Xunit;

[CollectionDefinition("WebApplicationFactory")]
public class WebApplicationFactoryCollection : ICollectionFixture<WebApplicationFactoryFixture>
{
}
