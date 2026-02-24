using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ProcureHub.Infrastructure.Database;
using Xunit;

namespace ProcureHub.Application.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>, ITestOutputHelperAccessor
{
    private static readonly Lock Lock = new();
    private static bool _databaseInitialized;

    public ITestOutputHelper? OutputHelper { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Sent env to "Test" to skip seeding data in the Blazor app's Program.cs
        builder.UseEnvironment("Test");

        // Load test appsettings.json to override Blazor appsettings (primarily for test DB connection string)
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile(
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
                optional: false,
                reloadOnChange: false);
        });

        builder.ConfigureLogging(logging => { logging.AddXUnit(this); });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            var migrationsAssembly = typeof(ProcureHub.Infrastructure.Migrations.Initial).Assembly.GetName().Name;
            
            // Add SQL Server database for tests
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly))
                    .EnableSensitiveDataLogging());

            // Create & migrate database
            lock (Lock)
            {
                if (!_databaseInitialized)
                {
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    db.Database.Migrate();

                    _databaseInitialized = true;
                }
            }
        });
    }
}
