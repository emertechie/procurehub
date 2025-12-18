using System.Net;
using ProcureHub.WebApi.Tests.Infrastructure;

namespace ProcureHub.WebApi.Tests;

[Collection("Integration Tests")]
public class ConfigurationTests(ITestOutputHelper testOutputHelper, IntegrationTestFixture fixture)
    : IntegrationTestsBase(testOutputHelper, fixture)
{
    /// <summary>
    /// Verifies the use of `webApp.UseStatusCodePages` in API config.
    /// </summary>
    [Fact]
    public async Task UseStatusCodePages_is_correctly_configured()
    {
        // Try fetch a non-existent endpoint
        var notFoundResp = await HttpClient.GetAsync("/does-not-exist", CancellationToken);

        // Assert response is in ProblemDetails format, and not just an empty body 
        await notFoundResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            CancellationToken,
            "Not Found");
    }
}