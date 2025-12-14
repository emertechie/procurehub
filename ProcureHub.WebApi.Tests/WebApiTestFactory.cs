using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ProcureHub;

namespace ProcureHub.WebApi.Tests;

public class WebApiTestFactory(ITestOutputHelper outputHelper) : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    public static readonly string AdminEmail = "test-admin@supporthub.local";
    public static readonly string AdminPassword = "TestAdmin123!";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test configuration for admin user
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DevAdminUser:Email"] = AdminEmail,
                ["DevAdminUser:Password"] = AdminPassword
            });
        });

        builder.ConfigureLogging(logging =>
        {
            logging.AddXUnit(outputHelper);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            // Create in-memory SQLite connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Add in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection)
                    .EnableSensitiveDataLogging());

            // Build service provider and create database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Close();
        _connection?.Dispose();
    }
}