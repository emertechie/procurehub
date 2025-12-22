using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ProcureHub.WebApi.Tests.Infrastructure.Xunit;

namespace ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;

/// <summary>
/// Creates a default HttpClient instance and provides convenience methods
/// to login and set required authentication headers on the HttpClient.
/// </summary>
public abstract class HttpClientBase
{
    public const string AdminEmail = "test-admin@procurehub.local";
    public const string AdminPassword = "TestAdmin123!";

    protected readonly ApiTestHost ApiTestHost;
    protected readonly HttpClient HttpClient;

    protected HttpClientBase(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    {
        ApiTestHost = hostFixture.ApiTestHost;
        ApiTestHost.OutputHelper = testOutputHelper;
        HttpClient = ApiTestHost.CreateClient();
    }

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    protected async Task LoginAsAdminAsync()
    {
        await LoginAsync(AdminEmail, AdminPassword);
    }

    protected async Task LoginAsync(string email, string password)
    {
        var loginRequest = JsonContent.Create(new { email, password });
        var loginResp = await HttpClient.PostAsync("/login", loginRequest, CancellationToken);
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);
        var loginResult = await loginResp.Content.ReadFromJsonAsync<LoginResponse>(CancellationToken);

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
    }

    private record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, string RefreshToken);
}
