using Microsoft.AspNetCore.Identity;
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

    public static async Task SeedDataAsync(
        IServiceProvider factoryServices,
        string adminEmail,
        string adminPassword)
    {
        using var scope = factoryServices.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();

        await DataSeeder.SeedAsync(dbContext, userManager, roleManager, logger, adminEmail, adminPassword);
    }
}
