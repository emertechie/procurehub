using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ProcureHub;
using ProcureHub.Constants;
using ProcureHub.Data;
using ProcureHub.Features.Staff;
using ProcureHub.Infrastructure;
using ProcureHub.Models;
using ProcureHub.WebApi;
using ProcureHub.WebApi.Authentication;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Helpers;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

// Customize FluentValidation messages
ValidatorOptions.Global.LanguageManager = new CustomLanguageManager();

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
    appBuilder.Services.AddOpenApi(options => { options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1; });

    var connectionString = appBuilder.Configuration.GetConnectionString("DefaultConnection") ??
                           throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    appBuilder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, dbOptions => dbOptions.MigrationsAssembly("ProcureHub")));

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
        // Policy that accepts any valid authentication method (Bearer token, API Key, or Cookie)
        options.AddPolicy(AuthorizationPolicyNames.Authenticated, policy =>
        {
            policy.AddAuthenticationSchemes(
                IdentityConstants.BearerScheme,
                IdentityConstants.ApplicationScheme,
                ApiKeyAuthenticationOptions.DefaultScheme);
            policy.RequireAuthenticatedUser();
        });

        // Role-based policies
        options.AddPolicy(RolePolicyNames.AdminOnly, policy => { policy.RequireRole(RoleNames.Admin); });
    });

    appBuilder.Services.AddRequestHandlers();

    // Register all FluentValidation validators 
    appBuilder.Services.AddValidatorsFromAssemblyContaining<CreateStaff.Request>();

    // Automatically run FluentValidation validators on ASP.Net Minimal APIs:
    builder.Services.AddFluentValidationAutoValidation();
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

        app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });

        using var scope = app.Services.CreateScope();

        // Ensure DB created
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

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

    ConfigureIdentityApiEndpoints(app);
}

void ConfigureIdentityApiEndpoints(WebApplication app)
{
    // Map Identity API endpoints (login, register, refresh, etc.)
    var identityEndpointsConventionBuilder = app.MapIdentityApi<ApplicationUser>()
        .AddOpenApiOperationTransformer(async (operation, context, _) =>
        {
            // Transform /login endpoint to document the ProblemHttpResult 401 response which is
            // not included by default. See: https://github.com/dotnet/aspnetcore/issues/52424
            if (context.Description is { HttpMethod: "POST", RelativePath: "login" })
            {
                operation.Responses ??= new OpenApiResponses();
                operation.Responses.TryAdd("401", new OpenApiResponse
                {
                    Description = "Unauthorized",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new()
                        {
                            Schema = new OpenApiSchemaReference("ProblemDetails", context.Document)
                        }
                    }
                });
            }
        });

    // The `MapIdentityApi` call doesn't add a `/logout` endpoint so add one here:
    app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }).RequireAuthorization();

    // NOTE: Crude approach for demo app to block self-registration. (Only admins can create / invite staff)
    BlockRegisterEndpoint(identityEndpointsConventionBuilder);
}

void BlockRegisterEndpoint(IEndpointConventionBuilder endpointConventionBuilder)
{
    endpointConventionBuilder.Add(endpointBuilder =>
    {
        if (endpointBuilder is RouteEndpointBuilder { RoutePattern.RawText: "/register" } routeEndpointBuilder)
        {
            // Clear existing filters and replace with a blocking filter
            routeEndpointBuilder.FilterFactories.Clear();

            // Return 404 to hide the endpoint's existence
            routeEndpointBuilder.FilterFactories.Add((context, next) =>
                invocationContext => new ValueTask<object?>(Results.NotFound()));
        }
    });
}
