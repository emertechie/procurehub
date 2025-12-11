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

// Turn unhandled exceptions into ProblemDetails response:
webApp.UseExceptionHandler(exceptionHandler 
    => exceptionHandler.Run(async context => await Results.Problem().ExecuteAsync(context)));

webApp.Run();

return;

void RegisterServices(WebApplicationBuilder webApplicationBuilder)
{
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
    // Configure the HTTP request pipeline.
    if (webApplication.Environment.IsDevelopment())
    {
        webApplication.MapOpenApi();
    }

    webApplication.UseHttpsRedirection();
    
    webApplication.MapIdentityApi<IdentityUser>();
}