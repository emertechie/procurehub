using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ProcureHub.WebApi.Tests.Infrastructure;

public class WebApiTestHost(string connectionString)
    : WebApplicationFactory<Program>, ITestOutputHelperAccessor
{
    private static readonly Lock _lock = new();
    private static bool _databaseInitialized;

    public ITestOutputHelper? OutputHelper { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Sent env to "Test" to skip seeding data in the API's Program.cs - since
        // any data seeded there is wiped out in the `ResetDatabaseAsync` call.
        builder.UseEnvironment("Test");

        builder.ConfigureLogging(logging => { logging.AddXUnit(this); });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            // Add Postgres database for tests
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, options => options.MigrationsAssembly("ProcureHub"))
                    .EnableSensitiveDataLogging());

            // Build service provider and create database
            lock (_lock)
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