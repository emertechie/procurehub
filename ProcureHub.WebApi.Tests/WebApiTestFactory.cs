using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ProcureHub.WebApi.Tests;

public class WebApiTestFactory(ITestOutputHelper outputHelper) : WebApplicationFactory<Program>
{
    private static string? _connectionString;
    private static bool _databaseInitialized;
    private static readonly object _lock = new();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Sent env to "Test" to skip seeding data in the API's Program.cs - since
        // any data seeded there is wiped out in the `ResetDatabaseAsync` call.
        builder.UseEnvironment("Test");

        builder.ConfigureLogging(logging =>
        {
            logging.AddXUnit(outputHelper);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            // Add Postgres database for tests
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(GetConnectionString())
                    .EnableSensitiveDataLogging());

            // Build service provider and create database
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.EnsureCreated();
                    _databaseInitialized = true;
                }
            }
        });
    }

    public static string GetConnectionString()
    {
        if (_connectionString != null)
            return _connectionString;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        return _connectionString;
    }
}