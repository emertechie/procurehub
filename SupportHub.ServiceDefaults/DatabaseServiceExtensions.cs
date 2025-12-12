// SupportHub.Infrastructure/DatabaseServiceExtensions.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SupportHub.ServiceDefaults;

public static class DatabaseServiceExtensions
{
    /// <summary>
    /// Adds database services using the configured provider
    /// </summary>
    public static IServiceCollection AddSupportHubDatabase(
        this IServiceCollection services, 
        Action<DbContextOptionsBuilder> optionsAction)
    {
        services.AddDbContext<ApplicationDbContext>(optionsAction);
        
        // TODO: enable in dev environment only
        // services.AddDatabaseDeveloperPageExceptionFilter();
        
        return services;
    }
    
    /// <summary>
    /// Adds database with SQLite (for development/testing)
    /// </summary>
    public static IServiceCollection AddSupportHubDatabaseWithSqlite(
        this IServiceCollection services,
        string connectionString)
    {
        return services.AddSupportHubDatabase(options =>
            options.UseSqlite(connectionString, dbOptions =>
                dbOptions.MigrationsAssembly("SupportHub")));
    }

    /*/// <summary>
    /// Adds database with SQL Server for production use
    /// </summary>
    public static IServiceCollection AddSupportHubDatabaseWithSqlServer(
        this IServiceCollection services,
        string connectionString)
    {
        return services.AddSupportHubDatabase(options =>
            options.UseSqlServer(connectionString));
    }*/
}