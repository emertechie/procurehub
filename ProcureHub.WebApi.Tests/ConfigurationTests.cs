using System.Net;
using ProcureHub.WebApi.Tests.Features;

namespace ProcureHub.WebApi.Tests;

public class ConfigurationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTestsBase(testOutputHelper)
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
            title: "Not Found");
    }
}