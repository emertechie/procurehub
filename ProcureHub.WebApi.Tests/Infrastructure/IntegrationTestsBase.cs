using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ProcureHub.WebApi.Tests.Infrastructure;

public abstract class IntegrationTestsBase : IAsyncLifetime
{
    protected const string AdminEmail = "test-admin@procurehub.local";
    protected const string AdminPassword = "TestAdmin123!";
    private readonly IntegrationTestFixture _fixture;

    protected readonly HttpClient HttpClient;

    protected IntegrationTestsBase(ITestOutputHelper testOutputHelper, IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.WebApiTestHost.OutputHelper = testOutputHelper;

        HttpClient = _fixture.WebApiTestHost.CreateClient();
    }

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        Console.WriteLine("*** In IntegrationTestsBase.InitializeAsync. Resetting database");

        await DatabaseResetter.ResetDatabaseAsync();
        await DatabaseResetter.SeedDataAsync(_fixture.WebApiTestHost.Services, AdminEmail, AdminPassword);
    }

    public ValueTask DisposeAsync()
    {
        Console.WriteLine("*** In IntegrationTestsBase.DisposeAsync");
        return ValueTask.CompletedTask;
    }

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
