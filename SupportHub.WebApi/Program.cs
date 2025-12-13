using Microsoft.AspNetCore.Identity;
using SupportHub;
using SupportHub.Constants;
using SupportHub.Data;
using SupportHub.Features.Staff.Registration;
using SupportHub.Infrastructure;
using SupportHub.Models;
using SupportHub.ServiceDefaults;
using SupportHub.WebApi;
using SupportHub.WebApi.Authentication;
using SupportHub.WebApi.Helpers;

var builder = WebApplication.CreateBuilder(args);
RegisterServices(builder);

var webApp = builder.Build();
await ConfigureApplication(webApp);

ApiEndpoints.Configure(webApp);

webApp.Run();

return;

void RegisterServices(WebApplicationBuilder appBuilder)
{
    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance =
                $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

            context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
        };
    });

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    appBuilder.Services.AddOpenApi();

    var connectionString = appBuilder.Configuration.GetConnectionString("DefaultConnection") ??
                           throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    appBuilder.Services.AddSupportHubDatabaseWithSqlite(connectionString);

    // Configure Identity with API endpoints (automatically adds Bearer token authentication)
    builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
        {
            options.Stores.MaxLengthForKeys = 128;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    // Add API Key authentication scheme (AddIdentityApiEndpoints already added Bearer token)
    builder.Services.AddAuthentication()
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationOptions.DefaultScheme,
            options => { });

    // Register API Key validator
    builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

    // Configure Authorization with flexible policies
    builder.Services.AddAuthorization(options =>
    {
        // Policy that accepts either Bearer token or API Key
        options.AddPolicy(AuthorizationPolicyNames.ApiKeyOrUserAccess, policy =>
        {
            policy.AddAuthenticationSchemes(
                IdentityConstants.BearerScheme,
                ApiKeyAuthenticationOptions.DefaultScheme);
            policy.RequireAuthenticatedUser();
        });

        // Policy for API Keys only
        options.AddPolicy(AuthorizationPolicyNames.ApiKeyOnly, policy =>
        {
            policy.AddAuthenticationSchemes(ApiKeyAuthenticationOptions.DefaultScheme);
            policy.RequireAuthenticatedUser();
        });

        // Policy for Bearer token only (for user-specific actions)
        options.AddPolicy(AuthorizationPolicyNames.UserOnly, policy =>
        {
            policy.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
            policy.RequireAuthenticatedUser();
        });

        // Role-based policies
        options.AddPolicy(RolePolicyNames.AdminOnly, policy =>
        {
            policy.RequireRole(RoleNames.Admin);
        });

        options.AddPolicy(RolePolicyNames.StaffOrAdmin, policy =>
        {
            policy.RequireRole(RoleNames.Admin, RoleNames.Staff);
        });
    });

    appBuilder.Services.AddRequestHandlers();
    
    // Filter that customizes the /register endpoint handling
    appBuilder.Services.AddScoped<RegistrationFilter>();
    appBuilder.Services.AddScoped<IRegistrationValidator, DummyRegistrationValidator>();
}

async Task ConfigureApplication(WebApplication app)
{
    // Writes a ProblemDetails response for status codes between 400 and 599 that do not have a body
    app.UseStatusCodePages();

    // Turn unhandled exceptions into ProblemDetails response:
    app.UseExceptionHandler();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        
        using var scope = app.Services.CreateScope();

        // Ensure DB created
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();

        // Seed database with roles and initial admin user
        await DataSeeder.SeedAsync(
            dbContext,
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>(),
            scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>(),
            app.Configuration.GetRequiredString("DevAdminUser:Email"),
            app.Configuration.GetRequiredString("DevAdminUser:Password"));
    }

    app.UseHttpsRedirection();

    // Add authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    // Map Identity API endpoints (login, register, refresh, etc.)
    var identityEndpointsConventionBuilder = app.MapIdentityApi<ApplicationUser>();
        
    CustomizeRegistrationLogic(identityEndpointsConventionBuilder);
}

// Use an endpoint filter to customize the /register endpoint logic
// (Only allow invited users and automatically create a Staff record)
void CustomizeRegistrationLogic(IEndpointConventionBuilder endpointConventionBuilder)
{
    endpointConventionBuilder.Add(endpointBuilder =>
    {
        if (endpointBuilder is RouteEndpointBuilder { RoutePattern.RawText: "/register" } routeEndpoint)
        {
            endpointBuilder.FilterFactories.Add((context, next) => invocationContext =>
            {
                var filter = invocationContext.HttpContext.RequestServices.GetRequiredService<RegistrationFilter>();
                return filter.InvokeAsync(invocationContext, next);
            });
        }
    });
}
