using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;

namespace ProcureHub.WebApi.Tests.Infrastructure;

// ReSharper disable once ClassNeverInstantiated.Global
public class IntegrationTestFixture : IAsyncLifetime
{
    private Respawner? _respawner;

    public string ConnectionString { get; } = GetConnectionString();

    public WebApiTestFactory WebApiTestFactory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // Initialize the API host once for all tests
        WebApiTestFactory = new WebApiTestFactory(ConnectionString);

        // Initialize the respawner once for all tests
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public ValueTask DisposeAsync()
    {
        WebApiTestFactory?.Dispose();
        return ValueTask.CompletedTask;
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner == null)
        {
            throw new InvalidOperationException("Respawner not initialized");
        }

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    private static string GetConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        return configuration.GetConnectionString("DefaultConnection") ??
               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }
}