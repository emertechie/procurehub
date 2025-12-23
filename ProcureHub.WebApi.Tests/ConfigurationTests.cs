using System.Net;
using ProcureHub.WebApi.Tests.Infrastructure;
using ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;
using ProcureHub.WebApi.Tests.Infrastructure.Xunit;

namespace ProcureHub.WebApi.Tests;

[Collection("ApiTestHost")]
public class ConfigurationTests(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    : HttpClientAndDbResetBase(hostFixture, testOutputHelper)
{
    /// <summary>
    /// Verifies the use of `webApp.UseStatusCodePages` in API config.
    /// </summary>
    [Fact]
    public async Task UseStatusCodePages_is_correctly_configured()
    {
        // Try fetch a non-existent endpoint
        var notFoundResp = await HttpClient.GetAsync("/does-not-exist");

        // Assert response is in ProblemDetails format, and not just an empty body 
        await notFoundResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Not Found");
    }
}
