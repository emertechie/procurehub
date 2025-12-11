using System.Net;

namespace SupportHub.WebApi.Tests;

public class ConfigurationTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client;

    public ConfigurationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        
        var factory = new WebApiTestFactory(testOutputHelper);
        _client = factory.CreateClient();
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    
    /// <summary>
    /// Verifies the use of `webApp.UseStatusCodePages` in API config.
    /// </summary>
    [Fact]
    public async Task UseStatusCodePages_is_correctly_configured()
    {
        // Try fetch a non-existent endpoint
        var notFoundResp = await _client.GetAsync("/does-not-exist", CancellationToken);

        // Assert response is in ProblemDetails format, and not just an empty body 
        await notFoundResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            CancellationToken,
            title: "Not Found");
    }
}