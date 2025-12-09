using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupportHub.Data;
using SupportHub.Web.Components;
using SupportHub.Web.Components.Account;

var builder = WebApplication.CreateBuilder(args);

RegisterServices(builder);

var app = builder.Build();

ConfigureApplication(app);

app.Run();

return;

static void RegisterServices(WebApplicationBuilder webApplicationBuilder)
{
    // Add services to the container.
    webApplicationBuilder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    webApplicationBuilder.Services.AddCascadingAuthenticationState();
    webApplicationBuilder.Services.AddScoped<IdentityRedirectManager>();
    webApplicationBuilder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

    webApplicationBuilder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

    var connectionString = webApplicationBuilder.Configuration.GetConnectionString("DefaultConnection") ??
                           throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    webApplicationBuilder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
    webApplicationBuilder.Services.AddDatabaseDeveloperPageExceptionFilter();

    webApplicationBuilder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

    webApplicationBuilder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
}

static void ConfigureApplication(WebApplication webApplication)
{
    // Configure the HTTP request pipeline.
    if (webApplication.Environment.IsDevelopment())
    {
        webApplication.UseMigrationsEndPoint();
    }
    else
    {
        webApplication.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        webApplication.UseHsts();
    }

    webApplication.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    webApplication.UseHttpsRedirection();

    webApplication.UseAntiforgery();

    webApplication.MapStaticAssets();
    webApplication.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
    webApplication.MapAdditionalIdentityEndpoints();
}