using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using ProcureHub.Data;
using ProcureHub.Models;
using Respawn;

namespace ProcureHub.WebApi.Tests;

public abstract class IntegrationTestsBase : IAsyncLifetime
{
    protected readonly HttpClient HttpClient;
    private readonly WebApiTestFactory _factory;
    private Respawner? _respawner;
    private readonly string _connectionString;

    protected const string AdminEmail = "test-admin@procurehub.local";
    protected const string AdminPassword = "TestAdmin123!";

    protected IntegrationTestsBase(ITestOutputHelper testOutputHelper)
    {
        _connectionString = GetConnectionString();    
        _factory = new WebApiTestFactory(testOutputHelper, _connectionString);
        HttpClient = _factory.CreateClient();
    }

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ResetDatabaseAsync();
        await SeedData(_factory.Services);
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

        HttpClient.DefaultRequestHeaders.Authorization =  new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
    }

    private record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, string RefreshToken);
    
    private static string GetConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        return configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    private async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
        }

        await _respawner.ResetAsync(connection);
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
}