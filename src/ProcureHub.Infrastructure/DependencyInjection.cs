using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Application.Abstractions.Data;
using ProcureHub.Infrastructure.Database;

namespace ProcureHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        services.AddSqlServerDbContext<ApplicationDbContext>(connectionString);
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IDbConstraints, DbConstraints>();
        
        return services;
    }
    
    private static IServiceCollection AddSqlServerDbContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        string? migrationsAssembly = null)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.UseSqlServer(connectionString, dbOptions =>
            {
                if (!string.IsNullOrWhiteSpace(migrationsAssembly))
                {
                    dbOptions.MigrationsAssembly(migrationsAssembly);
                }
            });
        });

        return services;
    }
}
