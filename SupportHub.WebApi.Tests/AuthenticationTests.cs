using System.Net.Http.Json;

namespace SupportHub.WebApi.Tests;

public class AuthenticationTests(ITestOutputHelper testOutputHelper)
    : TestsBase(testOutputHelper)
{
    [Fact]
    public async Task Can_login_as_admin_and_use_API()
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

    [Fact]
    public async Task Can_register_an_approved_user_and_create_a_linked_Staff_entity()
    {
        await LoginAsAdminAsync();

        // Assert no existing Staff entities
        var listStaffResp1 = await Client.GetAsync("/staff", CancellationToken);
        var staffList1 = await listStaffResp1.AssertSuccessAndReadJsonAsync<object[]>(CancellationToken);
        Assert.Empty(staffList1!);

        // Try to register with "unknown@example.com" email -> should be rejected
        var unauthorizedRegRequest = JsonContent.Create(new { email = "unknown@example.com", password = "Test1234!" });
        var unauthorizedRegResp = await Client.PostAsync("/register", unauthorizedRegRequest, CancellationToken);

        await unauthorizedRegResp.AssertValidationProblemAsync(CancellationToken,
            errors: new Dictionary<string, string[]>
            {
                ["Email"] = ["This email is not authorized to register."]
            });

        // Try to register with "staff1@example.com" -> should work
        var authorizedRegRequest = JsonContent.Create(new { email = "staff1@example.com", password = "Test1234!" });
        var authorizedRegResp = await Client.PostAsync("/register", authorizedRegRequest, CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, authorizedRegResp.StatusCode);

        // Assert one Staff entity created
        var listStaffResp2 = await Client.GetAsync("/staff", CancellationToken);
        var staffList2 = await listStaffResp2.AssertSuccessAndReadJsonAsync<object[]>(CancellationToken);
        Assert.Single(staffList2!);
    }

    private record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, string RefreshToken);
}