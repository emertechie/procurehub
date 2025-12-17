using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ProcureHub.Features.Staff;
using ProcureHub.WebApi.ApiResponses;

namespace ProcureHub.WebApi.Tests;

public class AuthenticationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTestsBase(testOutputHelper)
{
    /// <summary>
    /// For demo purposes, just not allowing manual registration.
    /// </summary>
    [Fact]
    public async Task Cannot_manually_register()
    {
        await LoginAsAdminAsync();

        // Try to register -> endpoint should not be available
        var unauthorizedRegRequest = JsonContent.Create(new { email = "joe@example.com", password = "Test1234!" });
        var unauthorizedRegResp = await HttpClient.PostAsync("/register", unauthorizedRegRequest, CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, unauthorizedRegResp.StatusCode);
    }

    [Fact]
    public async Task Can_login_as_admin_and_use_API_with_token()
    {
        // Login as the test admin (using credentials defined in WebApiTestFactory)
        var loginRequest = JsonContent.Create(new { email = "test-admin@procurehub.local", password = "TestAdmin123!" });
        var loginResp = await HttpClient.PostAsync("/login", loginRequest, CancellationToken);
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        var loginResult = await loginResp.Content.ReadFromJsonAsync<LoginResponse>(CancellationToken);
        Assert.NotNull(loginResult);
        Assert.NotEmpty(loginResult.AccessToken);

        // Set up the Bearer header with token
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        // The /me endpoint not available for token access
        var meResp = await HttpClient.GetAsync("/me", CancellationToken);
        Assert.Equal(HttpStatusCode.OK, meResp.StatusCode);
        var meResult = await meResp.Content.ReadFromJsonAsync<MeResponse>(CancellationToken);
        Assert.NotEmpty(meResult!.Id);
        Assert.Equal("test-admin@procurehub.local", meResult!.Email);

        // Make sure can call the /staff endpoint
        var staffResp = await HttpClient.GetAsync("/staff", CancellationToken);
        Assert.Equal(HttpStatusCode.OK, staffResp.StatusCode);
    }

    [Fact]
    public async Task Can_login_as_admin_and_use_API_with_cookie()
    {
        // Login as the test admin using cookie authentication
        var loginRequest = JsonContent.Create(new { email = "test-admin@procurehub.local", password = "TestAdmin123!" });
        var loginResp = await HttpClient.PostAsync("/login?useCookies=true", loginRequest, CancellationToken);
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        // Cookie should be set automatically by HttpClient's cookie container
        // Test the /me endpoint which should work with cookie auth
        var meResp = await HttpClient.GetAsync("/me", CancellationToken);
        Assert.Equal(HttpStatusCode.OK, meResp.StatusCode);
        var meResult = await meResp.Content.ReadFromJsonAsync<MeResponse>(CancellationToken);
        Assert.NotNull(meResult);
        Assert.NotEmpty(meResult.Id);
        Assert.Equal("test-admin@procurehub.local", meResult.Email);

        // Make sure can call the /staff endpoint with cookie
        var staffResp = await HttpClient.GetAsync("/staff", CancellationToken);
        Assert.Equal(HttpStatusCode.OK, staffResp.StatusCode);
    }
    
    private record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, string RefreshToken);
    private record MeResponse(string Id, string Email);
}