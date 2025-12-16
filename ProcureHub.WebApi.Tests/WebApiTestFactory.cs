using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;

namespace ProcureHub.WebApi.Tests;

public class WebApiTestFactory(ITestOutputHelper outputHelper) : WebApplicationFactory<Program>
{
    private static string? _connectionString;
    private static Respawner? _respawner;
    private static bool _databaseInitialized;
    private static readonly object _lock = new();

    public static readonly string AdminEmail = "test-admin@supporthub.local";
    public static readonly string AdminPassword = "TestAdmin123!";

    public static async Task ResetDatabaseAsync()
    {
        var connectionString = GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
        }

        await _respawner.ResetAsync(connection);
    }

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

    private static string GetConnectionString()
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