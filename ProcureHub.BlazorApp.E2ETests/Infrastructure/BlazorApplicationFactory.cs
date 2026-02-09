using System.Diagnostics;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProcureHub.BlazorApp.E2ETests.Infrastructure;

/// <summary>
/// WebApplicationFactory that starts the BlazorApp on a real Kestrel port
/// so Playwright can reach it over HTTP.
/// Based on egil/BlazorTestingAZ DummyHost pattern.
/// </summary>
public sealed class BlazorApplicationFactory(
    string connectionString,
    Action<IWebHostBuilder>? configureWebHost = null) : WebApplicationFactory<Program>, ITestOutputHelperAccessor
{
    private static readonly Lock Lock = new();
    private static bool _databaseInitialized;

    private IHost? _host;

    public ITestOutputHelper? OutputHelper { get; set; }

    public override IServiceProvider Services
        => _host?.Services
           ?? throw new InvalidOperationException("Call StartAsync() first to start host.");

    /// <summary>
    /// The base URL of the running Kestrel server (e.g. "https://localhost:12345").
    /// Available after <see cref="StartAsync"/> has been called.
    /// </summary>
    public string ServerAddress => _host is not null
        ? ClientOptions.BaseAddress.ToString()
        : throw new InvalidOperationException("Call StartAsync() first to start host.");

    public async Task StartAsync()
    {
        // Triggers CreateHost() getting called.
        _ = base.Services;

        Debug.Assert(_host is not null);

        // Spins the host app up.
        await _host.StartAsync();

        // Extract the selected dynamic port out of the Kestrel server
        // and assign it onto the client options for convenience so it
        // "just works" as otherwise it'll be the default http://localhost
        // URL, which won't route to the Kestrel-hosted HTTP server.
        var server = _host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        ClientOptions.BaseAddress = addresses!.Addresses
            .Select(x => x.Replace("127.0.0.1", "localhost", StringComparison.Ordinal))
            .Select(x => new Uri(x))
            .Last();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Must use "Development" so MapStaticAssets() can find the
        // staticwebassets.development.json manifest and serve JS/CSS correctly.
        builder.UseEnvironment("Development");

        // Load test appsettings.json to override BlazorApp appsettings
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

            // Add SQL Server database for tests
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOpts => sqlOpts.MigrationsAssembly("ProcureHub"))
                    .EnableSensitiveDataLogging());

            // Build service provider and run migrations once
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

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureWebHost(webHostBuilder =>
        {
            configureWebHost?.Invoke(webHostBuilder);
            webHostBuilder.UseKestrel();
            webHostBuilder.UseUrls("https://127.0.0.1:0");
        });

        _host = builder.Build();

        // Return a dummy host so that WAF doesn't try to start
        // the real host a second time.
        return new DummyHost();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        _host?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Minimal IHost returned to satisfy WAF's expectations.
    /// Provides a fake IServer (TestServer) so WAF doesn't throw.
    /// </summary>
    private sealed class DummyHost : IHost
    {
        public IServiceProvider Services { get; }

        public DummyHost()
        {
            Services = new ServiceCollection()
                .AddSingleton<IServer>(s => new TestServer(s))
                .BuildServiceProvider();
        }

        public void Dispose()
        {
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
