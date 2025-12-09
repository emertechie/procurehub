using Microsoft.AspNetCore.Mvc;
using SupportHub.Features.Departments;
using SupportHub.Infrastructure;
using SupportHub.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
RegisterServices(builder);

var webApp = builder.Build();
ConfigureApplication(webApp);
ConfigureApiEndpoints(webApp);

webApp.Run();

return;

void ConfigureApiEndpoints(WebApplication app)
{
    app.MapGet("/departments", (
            [FromServices] IRequestHandler<ListDepartments.Request, ListDepartments.Response[]> handler,
            CancellationToken token
        ) => handler.HandleAsync(new ListDepartments.Request(), token))
        .WithName("GetDepartments");

    app.MapGet("/departments/{id:int}", async (
            int id,
            [FromServices] IRequestHandler<GetDepartment.Request, GetDepartment.Response?> handler,
            CancellationToken token) => await handler.HandleAsync(new GetDepartment.Request(id), token) is var response
                ? Results.Ok(response)
                : Results.NotFound())
        .WithName("GetDepartmentById");

    app.MapPost("/departments", async (
            CreateDepartment.Request request,
            [FromServices] IRequestHandler<CreateDepartment.Request, int> handler,
            CancellationToken token
        ) =>
        {
            var newId = await handler.HandleAsync(request, token);
            return Results.Created($"/departments/{newId}", null);
        })
        .WithName("CreateDepartment");
}

void RegisterServices(WebApplicationBuilder webApplicationBuilder)
{
    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    webApplicationBuilder.Services.AddOpenApi();

    var connectionString = webApplicationBuilder.Configuration.GetConnectionString("DefaultConnection") ??
                           throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    webApplicationBuilder.Services.AddSupportHubDatabaseWithSqlite(connectionString);
    
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
}