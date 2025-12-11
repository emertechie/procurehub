using Microsoft.AspNetCore.Identity;
using SupportHub.Data;
using SupportHub.Infrastructure;
using SupportHub.ServiceDefaults;
using SupportHub.WebApi;

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

    builder.Services.AddAuthorization();
    builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
        .AddEntityFrameworkStores<ApplicationDbContext>(); ;

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
    
    webApplication.MapIdentityApi<IdentityUser>();
}