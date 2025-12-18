using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProcureHub.Data;
using ProcureHub.Models;

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
        await _fixture.ResetDatabaseAsync();
        await SeedData(_fixture.WebApiTestHost.Services);
    }

    public ValueTask DisposeAsync()
    {
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

    private static async Task SeedData(IServiceProvider factoryServices)
    {
        using var scope = factoryServices.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();

        await DataSeeder.SeedAsync(dbContext, userManager, roleManager, logger, AdminEmail, AdminPassword);
    }

    private record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, string RefreshToken);
}