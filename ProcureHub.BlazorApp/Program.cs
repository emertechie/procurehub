using FluentValidation;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;
using ProcureHub.Application;
using ProcureHub.Application.Abstractions.Identity;
using ProcureHub.BlazorApp.Components;
using ProcureHub.BlazorApp.Components.Account;
using ProcureHub.BlazorApp.Components.Pages.Requests;
using ProcureHub.BlazorApp.Infrastructure;
using ProcureHub.BlazorApp.Infrastructure.Authentication;
using ProcureHub.Domain.Entities;
using ProcureHub.Infrastructure.Database;
using ProcureHub.Infrastructure.Hosting;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();
builder.Services.AddApplicationServices();
builder.Services.AddValidatorsFromAssemblyContaining<PurchaseRequestFormModelValidator>(ServiceLifetime.Singleton);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<ICurrentUserProvider, AuthStateCurrentUserProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies(options =>
    {
        // Unique cookie name to avoid conflicts with WebApi on localhost
        options.ApplicationCookie!.Configure(cookie => cookie.Cookie.Name = ".AspNetCore.Identity.BlazorApp");
    });

// Add Microsoft Entra ID (Azure AD) authentication using authorization code flow with certificate
builder.Services.AddAuthentication()
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

// Override the SignInScheme to use external identity scheme for the OIDC callback
// This must be done AFTER AddMicrosoftIdentityWebApp to override its defaults
builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = IdentityConstants.ExternalScheme;
});

var connectionString = DatabaseConnectionString.GetConnectionString(builder.Configuration);

builder.Services.AddSqlServerDbContext<ApplicationDbContext>(connectionString);

builder.Services.AddIdentityCore<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.MaxLengthForKeys = 128;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();

builder.AddServiceDefaults();
    
var app = builder.Build();

await app.ApplyMigrationsIfNeededAsync<ApplicationDbContext>();
await app.ApplySeedDataIfNeededAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapDefaultEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
