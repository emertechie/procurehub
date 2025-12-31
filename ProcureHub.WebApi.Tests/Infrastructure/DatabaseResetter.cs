using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using ProcureHub.Data;
using ProcureHub.Models;
using Respawn;

namespace ProcureHub.WebApi.Tests.Infrastructure;

public static class DatabaseResetter
{
    private static Respawner? _respawner;

    public static async Task ResetDatabaseAsync()
    {
        var connectionString = Configuration.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });

        await _respawner.ResetAsync(connection);
    }

    public static async Task SeedDataAsync(IServiceProvider factoryServices)
    {
        using var scope = factoryServices.CreateScope();

        var seeder = new DataSeeder(
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<Role>>(),
            scope.ServiceProvider.GetRequiredService<IConfiguration>(),
            scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>());

        await seeder.SeedAsync(onlySeedRolesAndAdminUser: true);
    }
}
