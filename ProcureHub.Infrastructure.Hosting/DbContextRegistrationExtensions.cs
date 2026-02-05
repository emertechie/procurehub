using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProcureHub.Infrastructure.Hosting;

public static class DbContextRegistrationExtensions
{
    public static IServiceCollection AddSqlServerDbContext<TContext>(
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
