using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProcureHub.Data;
using ProcureHub.Models;

namespace ProcureHub.Infrastructure.Hosting;

public static class HostingSeedExtensions
{
    public static async Task ApplySeedDataIfNeededAsync(
        this WebApplication app,
        string configKey = "SEED_DATA")
    {
        var shouldSeed = app.Environment.IsDevelopment()
            || app.Configuration.GetValue<bool>(configKey);

        if (!shouldSeed)
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var seeder = new DataSeeder(
            dbContext,
            scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<Role>>(),
            app.Configuration,
            scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>());
        await seeder.SeedAsync();
    }
}
