// ProcureHub.Infrastructure/DatabaseServiceExtensions.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProcureHub.ServiceDefaults;

public static class DatabaseServiceExtensions
{
    /// <summary>
    /// Adds database services using the configured provider
    /// </summary>
    public static IServiceCollection AddProcureHubDatabase(
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
    public static IServiceCollection AddProcureHubDatabaseWithSqlite(
        this IServiceCollection services,
        string connectionString)
    {
        return services.AddProcureHubDatabase(options =>
            options.UseSqlite(connectionString, dbOptions =>
                dbOptions.MigrationsAssembly("ProcureHub")));
    }

    /*/// <summary>
    /// Adds database with SQL Server for production use
    /// </summary>
    public static IServiceCollection AddProcureHubDatabaseWithSqlServer(
        this IServiceCollection services,
        string connectionString)
    {
        return services.AddProcureHubDatabase(options =>
            options.UseSqlServer(connectionString));
    }*/
}