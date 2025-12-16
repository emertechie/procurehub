using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using ProcureHub.Data;
using ProcureHub.Models;
using Respawn;

namespace ProcureHub.WebApi.Tests;

public class WebApiTestFactory(ITestOutputHelper outputHelper) : WebApplicationFactory<Program>
{
    private static string? _connectionString;
    private static Respawner? _respawner;
    private static bool _databaseInitialized;
    private static readonly object _lock = new();

    public static readonly string AdminEmail = "test-admin@procurehub.local";
    public static readonly string AdminPassword = "TestAdmin123!";

    public async Task ResetDatabaseAsync()
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
        
        await SeedData();
    }

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
    
    private async Task SeedData()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();

        await DataSeeder.SeedAsync(dbContext, userManager, roleManager, logger, AdminEmail, AdminPassword);
    }
}