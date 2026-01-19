using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FluentValidation;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ProcureHub;
using ProcureHub.Constants;
using ProcureHub.Data;
using ProcureHub.Features.Users;
using ProcureHub.Features.Users.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Infrastructure.Authentication;
using ProcureHub.Models;
using ProcureHub.WebApi;
using ProcureHub.WebApi.Authentication;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Features.Auth;
using ProcureHub.WebApi.Helpers;
using User = ProcureHub.Models.User;

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

    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

    // Configure JSON serialization to use string enum values
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    appBuilder.Services.AddOpenApi(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
        options.CreateSchemaReferenceId = CreateOpenApiSchemaReferenceId;

        // Transform enum schemas to use string values instead of integers
        options.AddSchemaTransformer((schema, context, cancellationToken) =>
        {
            var type = context.JsonTypeInfo.Type;

            // Handle nullable enums (e.g., PurchaseRequestStatus?)
            var underlyingType = Nullable.GetUnderlyingType(type);
            var enumType = underlyingType?.IsEnum == true ? underlyingType : (type.IsEnum ? type : null);

            if (enumType != null)
            {
                schema.Type = JsonSchemaType.String;
                schema.Enum = Enum.GetNames(enumType)
                    .Select(name => (JsonNode)JsonValue.Create(name)!)
                    .ToList();
            }

            return Task.CompletedTask;
        });
    });

    var connectionString = appBuilder.Configuration.GetConnectionString("DefaultConnection") ??
                           throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // Append password from separate env var (for Key Vault secret injection in production)
    var databasePassword = appBuilder.Configuration["DatabasePassword"];
    if (!string.IsNullOrWhiteSpace(databasePassword))
    {
        connectionString += $";Password={databasePassword}";
    }

    appBuilder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, dbOptions => dbOptions.MigrationsAssembly("ProcureHub")));

    // Configure Identity with API endpoints (automatically adds Bearer token authentication)
    builder.Services.AddIdentityApiEndpoints<User>(options =>
        {
            options.Stores.MaxLengthForKeys = 128;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    // Add custom user sign-in validator
    builder.Services.AddScoped<UserSigninValidator>();

    // Replace default SignInManager<TUser>
    builder.Services.AddScoped<SignInManager<User>, ApplicationSigninManager>();

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
        options.AddPolicy(RolePolicyNames.Admin, policy => { policy.RequireRole(RoleNames.Admin); });
        options.AddPolicy(RolePolicyNames.Requester, policy => { policy.RequireRole(RoleNames.Requester); });
        options.AddPolicy(RolePolicyNames.Approver, policy => { policy.RequireRole(RoleNames.Approver); });
    });

    appBuilder.Services.AddDomainServices();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();

    // Register all FluentValidation validators 
    appBuilder.Services.AddValidatorsFromAssemblyContaining<CreateUser.Request>();

    // Add health checks
    appBuilder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database");

    // Configure CORS and cookie for cross-origin requests (e.g. SWA frontend -> Container Apps API)
    appBuilder.Services.AddCors();

    var allowedOrigins = appBuilder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
    if (allowedOrigins.Length > 0)
    {
        appBuilder.Services.Configure<CorsOptions>(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        // SameSite=None required for cross-origin cookies; Secure is required for SameSite=None
        appBuilder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
    }
}

async Task ConfigureApplication(WebApplication app)
{
    // Writes a ProblemDetails response for status codes between 400 and 599 that do not have a body
    app.UseStatusCodePages();

    // Turn unhandled exceptions into ProblemDetails response:
    app.UseExceptionHandler();

    var shouldMigrate = app.Environment.IsDevelopment()
        || app.Configuration.GetValue<bool>("MIGRATE_DB_ON_STARTUP");

    if (shouldMigrate)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations complete");
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });
    }

    var shouldSeed = app.Environment.IsDevelopment()
        || app.Configuration.GetValue<bool>("SEED_DATA");
    if (shouldSeed)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Seed database with roles, users, and initial data
        var seeder = new DataSeeder(
            dbContext,
            scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<Role>>(),
            app.Configuration,
            scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>());
        await seeder.SeedAsync();
    }

    app.UseHttpsRedirection();

    // Enable CORS (must be before authentication/authorization)
    app.UseCors();

    // Add authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    ConfigureIdentityApiEndpoints(app);
    ConfigureHealthEndpoints(app);
}

void ConfigureHealthEndpoints(WebApplication app)
{
    // Basic liveness check - just confirms app is running
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false // Don't run any checks, just return healthy if app is running
    }).ExcludeFromDescription();

    // Readiness check - includes database connectivity
    app.MapHealthChecks("/health/ready").ExcludeFromDescription();
}

void ConfigureIdentityApiEndpoints(WebApplication app)
{
    // Map Identity API endpoints (login, register, refresh, etc.)
    var identityEndpointsConventionBuilder = app.MapIdentityApi<User>()
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
    app.MapPost("/logout", async (SignInManager<User> signInManager) =>
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }).RequireAuthorization();

    // NOTE: Crude approach for demo app to block self-registration. (Only admins can create / invite users)
    // TODO: use derived `UserManager<User>` and override CreateAsync instead
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

// Nested types like `GetUserById.Response` and `GetDepartmentById.Response` were producing the same OpenApi
// schema name of DataResponseOfResponse for a return type like `DataResponse<GetUserById.Response>`. So the
// code below ensures the type name is the combined parent + child class to generate unique response schemas.
string? CreateOpenApiSchemaReferenceId(JsonTypeInfo jsonTypeInfo)
{
    var type = jsonTypeInfo.Type;

    // Handle generic types like DataResponse<T>, PagedResponse<T>
    if (type.IsGenericType)
    {
        var genericTypeName = type.Name.Split('`')[0]; // Gets "DataResponse" from "DataResponse`1"
        var genericArgs = type.GetGenericArguments();

        // Build schema name from generic arguments
        var argNames = string.Join("", genericArgs.Select(arg =>
        {
            // If the generic arg is a nested type, use DeclaringType + Type name
            if (arg.DeclaringType != null)
            {
                return $"{arg.DeclaringType.Name}{arg.Name}";
            }

            // Handle arrays
            if (arg.IsArray)
            {
                var elementType = arg.GetElementType()!;
                var elementName = elementType.DeclaringType != null
                    ? $"{elementType.DeclaringType.Name}{elementType.Name}"
                    : elementType.Name;
                return $"{elementName}Array";
            }

            return arg.Name;
        }));

        return $"{genericTypeName}Of{argNames}";
    }

    // For nested types, use DeclaringType + Type name
    if (type.DeclaringType != null)
    {
        return $"{type.DeclaringType.Name}{type.Name}";
    }

    return OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
}
