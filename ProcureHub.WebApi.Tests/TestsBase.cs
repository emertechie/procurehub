using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ProcureHub.WebApi.Tests;

public abstract class TestsBase
{
    protected readonly HttpClient Client;

    protected TestsBase(ITestOutputHelper testOutputHelper)
    {
        var factory = new WebApiTestFactory(testOutputHelper);
        Client = factory.CreateClient();
    }

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    protected async Task LoginAsAdminAsync()
    {
        var loginRequest = JsonContent.Create(new
        {
            email = WebApiTestFactory.AdminEmail,
            password = WebApiTestFactory.AdminPassword
        });
        var loginResp = await Client.PostAsync("/login", loginRequest, CancellationToken);
        var loginResult = await loginResp.Content.ReadFromJsonAsync<LoginResponse>(CancellationToken);

        Client.DefaultRequestHeaders.Authorization =  new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
    }

    private record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, string RefreshToken);
}