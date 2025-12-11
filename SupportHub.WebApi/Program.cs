using Microsoft.AspNetCore.Identity;
using SupportHub.Data;
using SupportHub.Infrastructure;
using SupportHub.ServiceDefaults;
using SupportHub.WebApi;
using SupportHub.WebApi.Authentication;

var builder = WebApplication.CreateBuilder(args);
RegisterServices(builder);

var webApp = builder.Build();
ConfigureApplication(webApp);

ApiEndpoints.Configure(webApp);

webApp.Run();

return;

void RegisterServices(WebApplicationBuilder webApplicationBuilder)
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
    webApplicationBuilder.Services.AddOpenApi();

    var connectionString = webApplicationBuilder.Configuration.GetConnectionString("DefaultConnection") ??
                           throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    webApplicationBuilder.Services.AddSupportHubDatabaseWithSqlite(connectionString);

    // Configure Identity with API endpoints
    builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    // Configure Authentication with multiple schemes
    builder.Services.AddAuthentication(options =>
        {
            // Use IdentityConstants.BearerScheme as default for Identity integration
            options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
            options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
        })
        .AddBearerToken(IdentityConstants.BearerScheme) // This is what Identity API uses
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationOptions.DefaultScheme,
            options => {});

    // Register API Key validator
    builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

    // Configure Authorization with flexible policies
    builder.Services.AddAuthorization(options =>
    {
        // Policy that accepts either Bearer token or API Key
        options.AddPolicy("ApiAccess", policy =>
        {
            policy.AddAuthenticationSchemes(
                IdentityConstants.BearerScheme,
                ApiKeyAuthenticationOptions.DefaultScheme);
            policy.RequireAuthenticatedUser();
        });

        // Policy for API Keys only
        options.AddPolicy("ApiKeyOnly", policy =>
        {
            policy.AddAuthenticationSchemes(ApiKeyAuthenticationOptions.DefaultScheme);
            policy.RequireAuthenticatedUser();
        });

        // Policy for Bearer token only (for user-specific actions)
        options.AddPolicy("UserOnly", policy =>
        {
            policy.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
            policy.RequireAuthenticatedUser();
        });
    });

    webApplicationBuilder.Services.AddRequestHandlers();
}

void ConfigureApplication(WebApplication webApplication)
{
    // Writes a ProblemDetails response for status codes between 400 and 599 that do not have a body
    webApp.UseStatusCodePages();

    // Turn unhandled exceptions into ProblemDetails response:
    webApp.UseExceptionHandler();

    // Configure the HTTP request pipeline.
    if (webApplication.Environment.IsDevelopment())
    {
        webApplication.MapOpenApi();
    }

    webApplication.UseHttpsRedirection();

    // Add authentication and authorization middleware
    webApplication.UseAuthentication();
    webApplication.UseAuthorization();

    // Map Identity API endpoints (login, register, refresh, etc.)
    webApplication.MapIdentityApi<ApplicationUser>();
}