using System.Net;
using System.Net.Http.Json;

namespace ProcureHub.WebApi.Tests;

public class AuthenticationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTestsBase(testOutputHelper)
{
    [Fact]
    public async Task Can_login_as_admin_and_use_API()
    {
        // Login as the test admin (using credentials defined in WebApiTestFactory)
        var loginRequest = JsonContent.Create(new { email = "test-admin@procurehub.local", password = "TestAdmin123!" });
        var loginResp = await HttpClient.PostAsync("/login", loginRequest, CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, loginResp.StatusCode);

        var loginResult = await loginResp.Content.ReadFromJsonAsync<LoginResponse>(CancellationToken);
        Assert.NotNull(loginResult);
        Assert.NotEmpty(loginResult.AccessToken);

        // Call the /staff endpoint and assert that the call is successful
        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var staffResp = await HttpClient.GetAsync("/staff", CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, staffResp.StatusCode);
    }

    [Fact]
    public async Task Cannot_manually_register()
    {
        await LoginAsAdminAsync();

        // Assert no existing Staff entities
        var listStaffResp1 = await HttpClient.GetAsync("/staff", CancellationToken);
        var staffList1 = await listStaffResp1.AssertSuccessAndReadJsonAsync<object[]>(CancellationToken);
        Assert.Empty(staffList1!);

        // Try to register -> endpoint should not be available
        var unauthorizedRegRequest = JsonContent.Create(new { email = "joe@example.com", password = "Test1234!" });
        var unauthorizedRegResp = await HttpClient.PostAsync("/register", unauthorizedRegRequest, CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, unauthorizedRegResp.StatusCode);
    }

    private record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, string RefreshToken);
}