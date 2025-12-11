using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SupportHub.Common;
using SupportHub.Data;
using SupportHub.Features.Departments;
using SupportHub.Features.Staff;
using SupportHub.Infrastructure;
using SupportHub.ServiceDefaults;
using SupportHub.WebApi;

var builder = WebApplication.CreateBuilder(args);
RegisterServices(builder);

var webApp = builder.Build();
ConfigureApplication(webApp);
ConfigureApiEndpoints(webApp);

webApp.Run();

return;

void ConfigureApiEndpoints(WebApplication app)
{
    // Staff
    app.MapGet("/staff", (
            [FromServices] IRequestHandler<ListStaff.Request, ListStaff.Response[]> handler,
            CancellationToken token
        ) => handler.HandleAsync(new ListStaff.Request(), token))
        .WithName("GetStaff")
        .WithTags("Staff");
    
    app.MapGet("/staff/{id}", async (
            string id,
            [FromServices] IRequestHandler<GetStaff.Request, GetStaff.Response?> handler,
            CancellationToken token) => await handler.HandleAsync(new GetStaff.Request(id), token) is var response
                ? Results.Ok(response)
                : Results.NotFound())
        .WithName("GetStaffById")
        .WithTags("Staff");

    app.MapPost("/staff", async (
            CreateStaff.Request request,
            [FromServices] IRequestHandler<CreateStaff.Request, Result<string>> handler,
            CancellationToken token
        ) =>
        {
            var result = await handler.HandleAsync(request, token);
            return result.Match(
                onSuccess: userId => Results.Created($"/staff/{userId}", new { userId }),
                onFailure: error => error.ToProblemDetails()
            );
        })
        .WithName("CreateStaff")
        .WithTags("Staff")
        .Produces<object>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

    // Departments
    app.MapGet("/departments", (
            [FromServices] IRequestHandler<ListDepartments.Request, ListDepartments.Response[]> handler,
            CancellationToken token
        ) => handler.HandleAsync(new ListDepartments.Request(), token))
        .WithName("GetDepartments")
        .WithTags("Departments");

    app.MapGet("/departments/{id:int}", async (
            int id,
            [FromServices] IRequestHandler<GetDepartment.Request, GetDepartment.Response?> handler,
            CancellationToken token) => await handler.HandleAsync(new GetDepartment.Request(id), token) is var response
                ? Results.Ok(response)
                : Results.NotFound())
        .WithName("GetDepartmentById")
        .WithTags("Departments");

    app.MapPost("/departments", async (
            CreateDepartment.Request request,
            [FromServices] IRequestHandler<CreateDepartment.Request, int> handler,
            CancellationToken token
        ) =>
        {
            var newId = await handler.HandleAsync(request, token);
            return Results.Created($"/departments/{newId}", null);
        })
        .WithName("CreateDepartment")
        .WithTags("Departments");
}

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