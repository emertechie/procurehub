using SupportHub.Infrastructure;
using SupportHub.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
RegisterServices(builder);

var app = builder.Build();
ConfigureApplication(app);
ConfigureApiEndpoints(app);

app.Run();

return;

void ConfigureApiEndpoints(WebApplication app1)
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    app1.MapGet("/weatherforecast", (ILogger<Program> logger) =>
        {
            logger.LogInformation("Hello from the WeatherForecast endpoint!");
            var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast");
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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}