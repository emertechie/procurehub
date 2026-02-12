using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace ProcureHub.BlazorApp.E2ETests.Infrastructure;

/// <summary>
/// Base class for Playwright E2E tests against ProcureHub.BlazorApp.
/// Starts a real Kestrel host, resets/seeds DB, and provides a Playwright Page.
/// Inherits from BrowserTest which manages Playwright browser lifecycle.
/// </summary>
public abstract class BlazorPageTest : BrowserTest
{
    public const string AdminEmail = "test-admin@example.com";
    public const string RequesterEmail = "test-requester@example.com";
    public const string ApproverEmail = "test-approver@example.com";
    public const string DefaultPassword = "Password1!";

    private BlazorApplicationFactory? _host;
    private IPage? _page;
    private IBrowserContext? _context;

    public IBrowserContext Context
        => _context ?? throw new InvalidOperationException("Setup has not been run.");

    public IPage Page
        => _page ?? throw new InvalidOperationException("Setup has not been run.");

    public BlazorApplicationFactory Host
        => _host ?? throw new InvalidOperationException("Setup has not been run.");

    /// <summary>Override to customize web host configuration per test class.</summary>
    protected virtual void ConfigureWebHost(IWebHostBuilder builder)
    {
    }

    /// <summary>Override to customize browser context options per test class.</summary>
    protected virtual BrowserNewContextOptions ContextOptions() => new();

    public override async ValueTask InitializeAsync()
    {
        // Auto-enable Playwright Inspector when a debugger is attached.
        // Must be set before base.InitializeAsync() launches the browser.
        if (Debugger.IsAttached)
        {
            Environment.SetEnvironmentVariable("PWDEBUG", "1");
        }

        var connectionString = Configuration.GetConnectionString();
        _host = new BlazorApplicationFactory(connectionString, ConfigureWebHost);
        await _host.StartAsync();
        
        // Reset and seed DB before each test
        await DatabaseResetter.ResetAndSeedAsync(_host.Services);

        // Initialize Playwright browser via BrowserTest
        await base.InitializeAsync();

        var options = ContextOptions();
        options.BaseURL = Host.ServerAddress;
        options.IgnoreHTTPSErrors = true;

        _context = await NewContext(options);
        _page = await Context.NewPageAsync();
        
        // Make things fail faster:
        _page.SetDefaultNavigationTimeout(3000);
        _page.SetDefaultTimeout(3000);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_page is not null)
        {
            await _page.CloseAsync();
        }

        if (_context is not null)
        {
            await _context.DisposeAsync();
        }

        if (_host is not null)
        {
            await _host.DisposeAsync();
            _host = null;
        }

        await base.DisposeAsync();
    }

    /// <summary>Navigate to login page and log in with given credentials.</summary>
    protected async Task LoginAsync(string email, string password)
    {
        await Page.GotoAsync("/Account/Login");
        await Page.FillAsync("[name='Input.Email']", email);
        await Page.FillAsync("[name='Input.Password']", password);
        await Page.ClickAsync("button[type='submit']");

        // Wait for navigation away from login page
        await Page.WaitForURLAsync(url => !url.Contains("/Account/Login"));
    }

    /// <summary>Login as the seeded admin user.</summary>
    protected Task LoginAsAdminAsync() => LoginAsync(AdminEmail, DefaultPassword);

    /// <summary>Login as the seeded requester user.</summary>
    protected Task LoginAsRequesterAsync() => LoginAsync(RequesterEmail, DefaultPassword);

    /// <summary>Login as the seeded approver user.</summary>
    protected Task LoginAsApproverAsync() => LoginAsync(ApproverEmail, DefaultPassword);
}
