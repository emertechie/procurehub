using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProcureHub.Data;
using ProcureHub.Models;
using Respawn;

namespace ProcureHub.BlazorApp.E2ETests.Infrastructure;

public static class DatabaseResetter
{
    private static Respawner? _respawner;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    /// <summary>
    /// Resets the database and seeds data atomically.
    /// Serialized via semaphore to prevent parallel test classes from
    /// racing on seed operations (duplicate key conflicts).
    /// </summary>
    public static async Task ResetAndSeedAsync(IServiceProvider factoryServices)
    {
        await Semaphore.WaitAsync();
        try
        {
            await ResetDatabaseAsync();
            await SeedDataAsync(factoryServices);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static async Task ResetDatabaseAsync()
    {
        var connectionString = Configuration.GetConnectionString();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });

        await _respawner.ResetAsync(connection);
    }

    private static async Task SeedDataAsync(IServiceProvider factoryServices)
    {
        using var scope = factoryServices.CreateScope();

        var seeder = new DataSeeder(
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<Role>>(),
            scope.ServiceProvider.GetRequiredService<IConfiguration>(),
            scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>());

        await seeder.SeedAsync(onlySeedRolesAndAdminUser: false);
    }
}
