namespace SupportHub.WebApi.Tests;

public abstract class TestsBase
{
    protected readonly HttpClient Client;

    protected TestsBase(ITestOutputHelper testOutputHelper)
    {
        var factory = new WebApiTestFactory(testOutputHelper);
        Client = factory.CreateClient();
    }

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}