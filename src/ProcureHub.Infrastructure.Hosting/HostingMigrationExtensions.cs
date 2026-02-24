using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProcureHub.Infrastructure.Hosting;

public static class HostingMigrationExtensions
{
    public static async Task ApplyMigrationsIfNeededAsync<TContext>(
        this WebApplication app,
        string configKey = "MIGRATE_DB_ON_STARTUP")
        where TContext : DbContext
    {
        var shouldMigrate = app.Environment.IsDevelopment()
            || app.Configuration.GetValue<bool>(configKey);

        if (!shouldMigrate)
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseMigration>>();

        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations complete");
    }

    private sealed class DatabaseMigration
    {
    }
}
