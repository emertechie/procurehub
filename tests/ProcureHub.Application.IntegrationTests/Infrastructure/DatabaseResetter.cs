using Microsoft.Data.SqlClient;
using Respawn;

namespace ProcureHub.Application.IntegrationTests.Infrastructure;

public static class DatabaseResetter
{
    private static Respawner? _respawner;

    public static async Task ResetDatabaseAsync()
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

    /*public static async Task SeedDataAsync(IServiceProvider factoryServices)
    {
        using var scope = factoryServices.CreateScope();

        var seeder = new DataSeeder(
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<Role>>(),
            scope.ServiceProvider.GetRequiredService<IConfiguration>(),
            scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>());

        await seeder.SeedAsync(onlySeedRolesAndAdminUser: true);
    }*/
}
