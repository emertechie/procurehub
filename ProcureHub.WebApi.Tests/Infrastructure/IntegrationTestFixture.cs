using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;

namespace ProcureHub.WebApi.Tests.Infrastructure;

// ReSharper disable once ClassNeverInstantiated.Global
public class IntegrationTestFixture : IAsyncLifetime
{
    private Respawner? _respawner;

    public string ConnectionString { get; } = GetConnectionString();

    public WebApiTestHost WebApiTestHost { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // Initialize the API host once for all tests
        WebApiTestHost = new WebApiTestHost(ConnectionString);
    }

    public ValueTask DisposeAsync()
    {
        WebApiTestHost?.Dispose();
        return ValueTask.CompletedTask;
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = ["__EFMigrationsHistory"]
            });
        }

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