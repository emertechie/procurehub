using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ProcureHub.Features.Roles;
using ProcureHub.WebApi.Responses;
using ProcureHub.WebApi.Tests.Infrastructure.Helpers;
using ProcureHub.WebApi.Tests.Infrastructure.Xunit;

namespace ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;

/// <summary>
/// Creates a default HttpClient instance and provides convenience methods
/// to login and set required authentication headers on the HttpClient.
/// </summary>
public abstract class HttpClientBase : IHttpClientAuthHelper
{
    // Note: Admin user gets seeded in ResetDatabaseFixture
    public const string AdminEmail = "test-admin@procurehub.local";
    public const string AdminPassword = ValidPassword;

    public const string ValidPassword = "Password1!";

    protected readonly ApiTestHost ApiTestHost;

    protected HttpClientBase(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    {
        ApiTestHost = hostFixture.ApiTestHost;
        ApiTestHost.OutputHelper = testOutputHelper;
        HttpClient = ApiTestHost.CreateClient();
    }

    public HttpClient HttpClient { get; init; }

    public async Task<string> CreateUserAsync(
        string email,
        string password,
        string[]? roleNames = null,
        Guid? departmentId = null
    )
    {
        return await UserHelper.CreateUserAsync(HttpClient, email, password, roleNames, departmentId);
    }

    public async Task AssignRoleToUserAsync(string userId, string roleName)
    {
        // First get role by name
        var getRolesResp = await HttpClient.GetAsync("/roles")
            .ReadJsonAsync<DataResponse<QueryRoles.Role[]>>();
        var role = getRolesResp.Data.FirstOrDefault(r => r.Name == roleName);
        Assert.NotNull(role);

        var request = JsonContent.Create(new { UserId = userId, RoleId = role.Id });
        var response = await HttpClient.PostAsync($"/users/{userId}/roles", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    public async Task LoginAsync(string email, string password)
    {
        var loginRequest = JsonContent.Create(new { email, password });
        var loginResp = await HttpClient.PostAsync("/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);
        var loginResult = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
    }

    public async Task LogoutAsync()
    {
        var logoutResp = await HttpClient.PostAsync("/logout", null);
        Assert.Equal(HttpStatusCode.OK, logoutResp.StatusCode);

        HttpClient.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task LoginAsAdminAsync()
    {
        await LoginAsync(AdminEmail, AdminPassword);
    }

    private record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, string RefreshToken);
}
