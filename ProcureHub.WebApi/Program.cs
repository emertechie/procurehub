using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FluentValidation;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ProcureHub;
using ProcureHub.Constants;
using ProcureHub.Features.Users;
using ProcureHub.Features.Users.Validation;
using ProcureHub.Infrastructure;
using ProcureHub.Infrastructure.Authentication;
using ProcureHub.Infrastructure.Hosting;
using ProcureHub.Models;
using ProcureHub.WebApi;
using ProcureHub.WebApi.Constants;
using ProcureHub.WebApi.Features.Auth;
using ProcureHub.WebApi.Infrastructure;
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
            ProblemDetailsHelper.ExtendWithHttpContext(context.ProblemDetails, context.HttpContext);
    });

    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

    var connectionString = DatabaseConnectionString.GetConnectionString(appBuilder.Configuration);

    appBuilder.Services.AddPostgresDbContext<ApplicationDbContext>(
        connectionString,
        migrationsAssembly: "ProcureHub");

    // Configure Identity with API endpoints (automatically adds Bearer token authentication)
    builder.Services.AddIdentityApiEndpoints<User>(options =>
        {
            options.Stores.MaxLengthForKeys = 128;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        // Unique cookie name to avoid conflicts with BlazorApp on localhost
        options.Cookie.Name = ".AspNetCore.Identity.WebApi";
        
        // Always return status codes rather than redirect:
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

    // Add custom user sign-in validator
    builder.Services.AddScoped<UserSigninValidator>();

    // Replace default SignInManager<TUser>
    builder.Services.AddScoped<SignInManager<User>, ApplicationSigninManager>();

    // Configure Authorization with flexible policies
    builder.Services.AddAuthorization(options =>
    {
        // Policy that accepts any valid authentication method (Bearer token, API Key, or Cookie)
        options.AddPolicy(AuthorizationPolicyNames.Authenticated, policy =>
        {
            policy.AddAuthenticationSchemes(
                IdentityConstants.BearerScheme,
                IdentityConstants.ApplicationScheme);
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
    appBuilder.Services.AddValidatorsFromAssemblyContaining<CreateUser.Command>();

    // Add health checks
    appBuilder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database");

    appBuilder.AddServiceDefaults();

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

    await app.ApplyMigrationsIfNeededAsync<ApplicationDbContext>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });
    }

    await app.ApplySeedDataIfNeededAsync();

    app.UseHttpsRedirection();

    // Enable CORS (must be before authentication/authorization)
    app.UseCors();

    // Add authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    ConfigureIdentityApiEndpoints(app);
    app.MapDefaultEndpoints();
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
    }).RequireAuthorization(AuthorizationPolicyNames.Authenticated);

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
