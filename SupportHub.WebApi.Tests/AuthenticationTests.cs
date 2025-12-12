using System.Net.Http.Json;

namespace SupportHub.WebApi.Tests;

public class AuthenticationTests(ITestOutputHelper testOutputHelper)
    : TestsBase(testOutputHelper)
{
    [Fact]
    public async Task Can_login_and_use_API()
    {
        // Login as the test admin (using credentials defined in WebApiTestFactory)
        var loginRequest = JsonContent.Create(new { email = "test-admin@supporthub.local", password = "TestAdmin123!" });
        var loginResp = await Client.PostAsync("/login", loginRequest, CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, loginResp.StatusCode);

        var loginResult = await loginResp.Content.ReadFromJsonAsync<LoginResponse>(CancellationToken);
        Assert.NotNull(loginResult);
        Assert.NotEmpty(loginResult.AccessToken);

        // Call the /staff endpoint and assert that the call is successful
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var staffResp = await Client.GetAsync("/staff", CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, staffResp.StatusCode);
    }

    private record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, string RefreshToken);
}